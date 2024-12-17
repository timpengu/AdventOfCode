using MoreLinq;
using System.Text.RegularExpressions;

internal static class Program
{
    public static void Main(string[] args)
    {
        List<(char Reg, long Value)> registers = new();
        List<int> program = new();

        foreach (string line in File.ReadLines("input.txt").Where(l => l.Length > 0))
        {
            if (line.TryParseRegister(out char reg, out long value))
            {
                registers.Add((reg,value));
            }
            else if (line.TryParseProgram(out program))
            {
            }
            else throw new InvalidDataException($"Unknown input: {line}");
        }

        var computer = new Computer() { IsVerbose = true };

        // part 1
        computer.InitRegisters(registers);
        List<int> output1 = computer.Execute(program).ToList();
        Console.WriteLine($"\nOutput: {string.Join(',', output1)}\n");

        // part 2
        List<long> aValues = [];
        for (int digit = 1; digit <= program.Count; ++digit)
        {
            var target = program[^digit..];
            Console.WriteLine($"Searching for sequence: {string.Join(",", target)}");
            aValues = FindGenerators(target, registers, program).ToList();
            Console.WriteLine($"Found generators: {string.Join(",", aValues.Select(a => a.ToOctalString()))}\n");
        }
        long aMin = aValues.Min();
        computer.InitRegisters(registers.WithRegisterA(aMin));
        List<int> output2 = computer.Execute(program).ToList();
        Console.WriteLine($"\nOutput: {string.Join(',', output2)}");
        Console.WriteLine($"Minimum value for self-replicataion: A={aMin} ({aMin.ToOctalString()})");
    }

    private static IEnumerable<(char Reg, long Value)> WithRegisterA(this IEnumerable<(char Reg, long)> registers, long aValue) => registers.Where(r => r.Reg != 'A').Append(('A', aValue));

    private static IEnumerable<long> FindGenerators(IEnumerable<int> target, IEnumerable<(char Reg, long Value)> registers, IReadOnlyList<int> program)
    {
        if (!target.Any())
        {
            return [0];
        }

        IEnumerable<long> candidates =
            from g in FindGenerators(target.Skip(1), registers, program) // find generators of truncated sequence
            from i in Enumerable.Range(0, 8)
            select (g << 3) + i; // extend sequence by one octal digit

        return candidates
            .OrderBy(g => g)
            .WhereGeneratesTarget(target, registers, program);
    }

    private static IEnumerable<long> WhereGeneratesTarget(this IEnumerable<long> aValues, IEnumerable<int> target, IEnumerable<(char Reg, long Value)> registers, IReadOnlyList<int> program)
    {
        var computer = new Computer();
        return aValues.Where(a =>
        {
            computer.InitRegisters(registers.WithRegisterA(a));
            return computer.Execute(program).SequenceEqual(target);
        });
    }

    private static bool TryParseRegister(this string line, out char reg, out long value)
    {
        Match match = Regex.Match(line, @"^Register ([A-Z]): ([0-9]+)$");
        if (match.Success &&
            match.Groups[1].Value.Length == 1 &&
            long.TryParse(match.Groups[2].Value, out value))
        {
            reg = match.Groups[1].Value.Single();
            return true;
        }

        (reg, value) = (default, default);
        return false;
    }

    private static bool TryParseProgram(this string line, out List<int> program)
    {
        Match match = Regex.Match(line, @"^Program: ([0-9, ]+)$");
        if (match.Success)
        {
            List<int?> values = match.Groups[1].Value.Split(',').Select(s => int.TryParse(s, out int value) ? (int?)value : null).ToList();
            if (values.All(v => v.HasValue))
            {
                program = values.Select(v => v.Value).ToList();
                return true;
            }
        }

        program = [];
        return false;
    }
}
