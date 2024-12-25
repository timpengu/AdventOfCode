using MoreLinq;
using System.Text.RegularExpressions;

internal static class Program
{
    private static void Main(string[] _)
    {
        Dictionary<string, bool> signals = new();
        List<Gate> gates = new();
        
        foreach (string line in File.ReadLines("input.txt").Where(l => l.Length > 0))
        {
            if (line.TryParseSignal(out string? signal, out bool value))
            {
                signals.Add(signal!, value);
            }
            else if (line.TryParseGate(out Gate gate))
            {
                gates.Add(gate);
            }
            else throw new InvalidDataException($"Unknown input: {line}");
        }

        // Part 1
        Circuit circuit = new(gates, signals, verbose:true);

        ulong x = circuit.GetRegister('x');
        ulong y = circuit.GetRegister('y');
        Console.WriteLine($"x: {x}");
        Console.WriteLine($"y: {y}\n");

        circuit.PropagateSignals();

        ulong z = circuit.GetRegister('z');
        Console.WriteLine($"\nOutput: {z}\n");

        // Part 2
        IList<(string, string)> swaps = FindSwaps(gates, 4);
        if (!swaps.Any())
        {
            Console.WriteLine("\nSolution not found.");
            return;
        }

        Console.WriteLine("\nSolution:");
        foreach (var swap in swaps)
        {
            Console.WriteLine($"{swap.Item1}-{swap.Item2}");
        }

        List<string> swapped = swaps.SelectMany(s => new[] { s.Item1, s.Item2 }).Order().ToList();
        Console.WriteLine($"\nSwapped signals: {String.Join(',', swapped)}");
    }

    private static IList<(string, string)> FindSwaps(IEnumerable<Gate> gates, int maxSwaps)
    {
        return FindSwaps(Enumerable.Empty<(string, string)>());

        IList<(string, string)> FindSwaps(IEnumerable<(string, string)> swaps, int bit = 0)
        {
            Circuit circuit = new(gates.WithSwaps(swaps));
            while (bit < Bits.Maximum && circuit.Test(bit))
            {
                ++bit;
            }

            Console.WriteLine($"[{bit}] With swaps: {String.Join(' ', swaps.Select(s => $"{s.Item1}-{s.Item2}"))}");
            circuit.Test(bit, verbose: true);

            if (bit == Bits.Maximum)
            {
                return swaps.ToList(); // matches all bits
            }

            if (swaps.Count() < maxSwaps)
            {
                int circuitDepth = 5;
                HashSet<string> depSignals = new();
                depSignals.UnionWith(circuit.GetDependents(Circuit.GetSignal('x', bit), circuitDepth));
                depSignals.UnionWith(circuit.GetDependents(Circuit.GetSignal('y', bit), circuitDepth));
                depSignals.UnionWith(circuit.GetDependencies(Circuit.GetSignal('z', bit), circuitDepth));
                depSignals.UnionWith(circuit.GetDependents(Circuit.GetSignal('x', bit + 1), circuitDepth));
                depSignals.UnionWith(circuit.GetDependents(Circuit.GetSignal('y', bit + 1), circuitDepth));
                depSignals.UnionWith(circuit.GetDependencies(Circuit.GetSignal('z', bit + 1), circuitDepth));
                depSignals.ExceptWith(swaps.Select(s => s.Item1));
                depSignals.ExceptWith(swaps.Select(s => s.Item2));
                Console.WriteLine($"[{bit}] Testing swaps with {String.Join(',', depSignals.Order())}...");

                foreach (var subset in depSignals.Subsets(2))
                {
                    var swap = (subset[0], subset[1]);
                    var newSwaps = swaps.Append(swap);
                    circuit = new(gates.WithSwaps(newSwaps));
                    if (circuit.Test(bit))
                    {
                        IList<(string, string)> result = FindSwaps(newSwaps, bit);
                        if (result.Any())
                            return result;
                    }
                }
            }

            Console.WriteLine($"[{bit}] Backtracking...");
            return [];
        }
    }

    private static IEnumerable<Gate> WithSwaps(this IEnumerable<Gate> gates, IEnumerable<(string, string)> swaps) =>
        swaps.Aggregate(gates, (gates, swap) => gates.WithSwap(swap));
    private static IEnumerable<Gate> WithSwap(this IEnumerable<Gate> gates, (string, string) swap)
    {
        foreach(var gate in gates)
        {
            yield return
                gate.Output == swap.Item1 ? gate with { Output = swap.Item2 } :
                gate.Output == swap.Item2 ? gate with { Output = swap.Item1 } :
                gate;
        }
    }

    private static bool Test(this Circuit circuit, int maxBit, bool verbose = false) => circuit.Test(MoreEnumerable.Sequence(0, maxBit), verbose);
    private static bool Test(this Circuit circuit, IEnumerable<int> testBits, bool verbose = false)
    {
        bool isOk = true;
        foreach(int bit in testBits)
        foreach (var testCase in GenerateTestCases(bit))
        {
            circuit.ResetSignals();
            circuit.SetRegister('x', testCase.X);
            circuit.SetRegister('y', testCase.Y);
            circuit.PropagateSignals();

            ulong outputActual = circuit.GetRegister('z');
            ulong outputExpected = testCase.X + testCase.Y;

            if (outputActual != outputExpected)
            {
                if (verbose)
                {
                    Console.WriteLine($"[{bit}] x:{testCase.X:x} + y:{testCase.Y:x} => {outputExpected:x} != z:{outputActual:x}");
                }
                isOk = false;
            }
        }
        return isOk;
    }

    private static IEnumerable<(ulong X, ulong Y)> GenerateTestCases(int bit)
    {
        return GetTestCases().Where(value =>
            value.X < Bits.OverflowValue &&
            value.Y < Bits.OverflowValue &&
            value.X + value.Y < Bits.OverflowValue);

        IEnumerable<(ulong X, ulong Y)> GetTestCases()
        {
            ulong b = bit.ToBitValue();
            yield return (b, 0);
            yield return (0, b);
            yield return (b, b);
        }
    }

    private static bool TryParseSignal(this string line, out string? signal, out bool value)
    {
        Match match = Regex.Match(line, @"^([A-Za-z0-9]+): ([01])$");
        if (match.Success)
        {
            signal = match.Groups[1].Value;
            value = match.Groups[2].Value == "1";
            return true;
        }

        (signal, value) = (default, default);
        return false;
    }

    private static bool TryParseGate(this string line, out Gate gate)
    {
        Match match = Regex.Match(line, @"^([A-Za-z0-9]+) (AND|OR|XOR) ([A-Za-z0-9]+) -> ([A-Za-z0-9]+)$");
        if (match.Success &&
            Enum.TryParse<BooleanOperator>(match.Groups[2].Value, ignoreCase:true, out BooleanOperator op))
        {
            string input1 = match.Groups[1].Value;
            string input2 = match.Groups[3].Value;
            string output = match.Groups[4].Value;
            gate = new(op, input1, input2, output);
            return true;
        }

        gate = default;
        return false;
    }
}
