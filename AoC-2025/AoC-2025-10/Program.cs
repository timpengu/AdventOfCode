using MoreLinq;
using System.Diagnostics;
using System.Text;

const string InputFile = "input.txt";
const string PyPath = "C:\\Python313\\python.exe";

bool useDFS1 = false;
bool useDFS2 = true;
bool usePy = false;

List<Machine> machines = File.ReadLines(InputFile)
    .Where(s => !String.IsNullOrEmpty(s) && !s.Trim().StartsWith("//"))
    .Select(Machine.Parse)
    .OrderBy(m => m.Buttons.Count)
    .ThenBy(m => m.Joltages.Length)
    .ToList();

int indicatorTotal = 0;
int joltageTotal = 0;

for (int m = 0; m < machines.Count; m++)
{
    var machine = machines[m];
    Console.WriteLine($"\n{m + 1}/{machines.Count}: {machine}");

    // Part 1

    List<int> indicatorSeq = FindIndicatorSequences(machine).First();
    Console.WriteLine($"Indicator sequence: {String.Join(",", indicatorSeq)}");

    indicatorTotal += indicatorSeq.Count;

    // Part 2

    if (useDFS1)
    {
        List<int> joltagePushes1 = FindJoltageButtonCounts1(machine).First();
        Console.WriteLine($"FindJoltageButtonCounts1 ({joltagePushes1.Sum()}): {String.Join(",", joltagePushes1)}");
    }

    if (useDFS2)
    {
        List<int> joltagePushes2 = FindJoltageButtonCounts2(machine).MinBy(p => p.Sum()) ?? throw new Exception("No solution");
        Console.WriteLine($"FindJoltageButtonCounts2 ({joltagePushes2.Sum()}): {String.Join(",", joltagePushes2)}");
    }

    List<int> joltagePushes3 = FindJoltageButtonCounts3(machine).MinBy(p => p.Sum()) ?? throw new Exception("No solution");
    Console.WriteLine($"FindJoltageButtonCounts3 ({joltagePushes3.Sum()}): {String.Join(",", joltagePushes3)}");

    joltageTotal += joltagePushes3.Sum();
}

Console.WriteLine();
Console.WriteLine($"Indicator total: {indicatorTotal}");
Console.WriteLine($"Joltage total: {joltageTotal}");

Dictionary<Machine, int> pySolutions = new();
if (usePy)
{
    foreach (var m in machines)
    {
        string py = BuildPy(m);
        string result = ExecPy(py);
        int solution = int.Parse(result.Trim());

        pySolutions[m] = solution;

        Console.WriteLine($"{m} => {solution}");
    }

    Console.WriteLine($"\nJoltage py total: {pySolutions.Values.Sum()}");
}

IEnumerable<List<int>> FindIndicatorSequences(Machine machine)
{
    var initialState = (machine.IndicatorMask, Enumerable.Empty<int>());
    Queue<(int Indicators, IEnumerable<int> Sequence)> queue = new([initialState]);
    HashSet<int> visited = [];
    while (queue.TryDequeue(out var state))
    {
        if (visited.Add(state.Indicators))
        {
            // Console.WriteLine($" => {String.Join(",", state.Sequence)}");

            if (state.Indicators == 0)
            {
                yield return state.Sequence.ToList();
            }
            else
            {
                foreach (var button in machine.Buttons)
                {
                    var nextState = (
                        Indicators: state.Indicators ^ button.Mask,
                        Sequence: state.Sequence.Append(button.ButtonIndex)
                    );
                    queue.Enqueue(nextState);
                }
            }
        }
    }
}

// Part 2 first DFS attempt:
// - order buttons by descending number of effected joltages
// - try each button a number of pushes, from the maximum without overflowing any joltage, down to zero
// - recursively try remaining buttons and backtrack if joltages are not reduced to zero
// Should find solution with minimum pushes first because buttons are tried in desending order of effect
// However this limits heuristic improvements and it's too slow for the large search spaces in the input
IEnumerable<List<int>> FindJoltageButtonCounts1(Machine machine)
{
    // Reorder buttons heuristically
    List<Button> buttons = machine.Buttons
        .OrderByDescending(b => b.JoltIndexes.Length)
        .ThenByDescending(b => b.MaxPushes(machine.Joltages))
        .ToList();

    return Solve(machine.Joltages, [], 0);

    IEnumerable<List<int>> Solve(int[] joltages, IEnumerable<(Button,int)> buttonPushes, int buttonIndex)
    {
        if (buttonIndex == buttons.Count)
        {
            if (joltages.All(j => j == 0))
            {
                yield return buttonPushes.ToPushCounts(machine.Buttons.Count).ToList();
            }
        }
        else
        {
            var button = buttons[buttonIndex];
            bool isLastButton = buttonIndex == buttons.Count - 1;
            
            int maxPushes = button.MaxPushes(joltages);
            int minPushes = isLastButton ? maxPushes : 0;

            for (int pushes = maxPushes; pushes >= minPushes; --pushes) // if any
            {
                (int[] nextJoltages, var nextButtonPushes) = (joltages, buttonPushes);
                if (pushes > 0)
                {
                    nextJoltages = joltages.EquiZip(button.Vector, (j, b) => j - pushes * b).ToArray();
                    nextButtonPushes = buttonPushes.Append((button, pushes));
                }

                var solutions = Solve(nextJoltages, nextButtonPushes, buttonIndex + 1);
                foreach (var solution in solutions)
                {
                    yield return solution;
                }
            }
        }
    }
}

// Part 2 second DFS attempt:
// Added an outer loop over joltages ordered by ascending number of influencing buttons, to assign highly-constrained variables early
// Faster but unfortunately has lost the property of finding minimal solutions first, so need to enumerate all solutions
// Still too slow to solve the hardest inputs with 13x10 matrices
IEnumerable<List<int>> FindJoltageButtonCounts2(Machine machine)
{
    // Order buttons by descending number of outputs
    Dictionary<int, Button[]> buttonsByJoltIndex = machine.Buttons
        .SelectMany(b => b.JoltIndexes, (b, j) => (Button: b, JoltIndex: j))
        .GroupBy(b => b.JoltIndex, b => b.Button)
        .ToDictionary(
            bs => bs.Key,
            bs => bs.OrderByDescending(b => b.JoltIndexes.Length).ToArray()
        );

    // Order joltIndexes by ascending number of inputs
    int[] joltIndexes = buttonsByJoltIndex
        .OrderBy(js => js.Value.Length)
        .Select(js => js.Key)
        .ToArray();

    return Solve(machine.Joltages, [], 0, 0);

    IEnumerable<List<int>> Solve(int[] joltages, IEnumerable<(Button,int)> buttonPushes, int j, int b)
    {
        // Console.WriteLine($"Solve: {{{String.Join(",", joltages)}}} j={j} b={b}");

        if (j == joltIndexes.Length)
        {
            if (joltages.All(j => j == 0))
            {
                yield return buttonPushes.ToPushCounts(machine.Buttons.Count).ToList();
            }
        }
        else
        {
            int joltIndex = joltIndexes[j];
            var buttons = buttonsByJoltIndex[joltIndex];
            var button = buttons[b];
            bool isLastButton = b == buttons.Length - 1;

            (int jNext, int bNext) = isLastButton ? (j + 1, 0) : (j, b + 1);

            int maxPushes = button.MaxPushes(joltages);
            int minPushes = isLastButton ? joltages[joltIndex] : 0;
            var rangePushes = minPushes > maxPushes ? [] : MoreEnumerable.Sequence(maxPushes, minPushes);
            var solutions = (j == 0 && b == 0)
                ? rangePushes .AsParallel().SelectMany(SolveButton)
                : rangePushes.SelectMany(SolveButton);

            foreach (var solution in solutions)
            {
                yield return solution;
            }

            IEnumerable<List<int>> SolveButton(int nextPushes)
            {
                (int[] nextJoltages, var nextButtonPushes) = (joltages, buttonPushes);
                if (nextPushes > 0)
                {
                    nextJoltages = joltages.EquiZip(button.Vector, (j, b) => j - nextPushes * b).ToArray();
                    nextButtonPushes = buttonPushes.Append((button, nextPushes));
                }

                return Solve(nextJoltages, nextButtonPushes, jNext, bNext);
            }
        }
    }
}

// Part 2 third DFS attempt
// Generalisation of part 1 finds button combinations with zero parity in lowest bit of joltages, then finds remaining bits recursively
// Much faster, but does not find minimal solutions first so need to enumerate all solutions
IEnumerable<List<int>> FindJoltageButtonCounts3(Machine machine)
{
    // TODO: Can improve heuristics to find shortest solution first? (avoid generating all solutions)
    // TODO: Use BFS to search solutions concurrently and terminate when the shortest is found?
    // TODO: Simplify this by removing the masks? (but then more array copies?)
    // TODO: Use a struct vector type?

    // Reorder buttons heuristically
    List<Button> buttons = machine.Buttons
        .OrderByDescending(b => b.JoltIndexes.Length)
        .ThenByDescending(b => b.MaxPushes(machine.Joltages))
        .ToList();

    return Solve(machine.Joltages, [], 0);

    IEnumerable<List<int>> Solve(int[] joltages, IEnumerable<(Button Button, int Pushes)> buttonPushes, int bit)
    {
        // Console.WriteLine($"{1 << bit} => {{{String.Join(",", joltages)}}} {String.Join(", ", buttonPushes.Select(b => $"{b.Button}:{b.Pushes}"))}");

        if (joltages.All(j => j == 0))
        {
            yield return buttonPushes.ToPushCounts(buttons.Count).ToList();
            yield break;
        }

        int mask = joltages.ToBitMask(bit);

        // test button combos in order of increasing length
        foreach (Button[] buttonCombo in buttons.OrderedCombinations())
        {
            int nextMask = buttonCombo.Aggregate(mask, (m, b) => m ^ b.Mask);
            if (nextMask == 0)
            {
                // found a combo that zeros the bit mask, update the joltages
                int[] nextJoltages = buttonCombo.Length == 0 ? joltages :
                    buttonCombo.Aggregate(
                        joltages.ToArray(),
                        (js, button) => js.ZipAssign(button.Vector, (j, v) => j - (v << bit)));

                if (nextJoltages.All(j => j >= 0))
                {
                    var nextButtonPushes = buttonCombo.Aggregate(buttonPushes, (p, b) => p.Append((b, 1 << bit)));                        
                    var solutions = Solve(nextJoltages, nextButtonPushes, bit + 1);
                    foreach (var solution in solutions)
                    {
                        yield return solution;
                    }
                }
            }
        }
    }
}

// Part 2 cheat by building a python/z3 linear algebra solver
static string BuildPy(Machine machine)
{
    int[] bs = machine.Buttons.Select(b => b.ButtonIndex).ToArray();

    StringBuilder sb = new();

    sb.AppendLine("from z3 import *");
    sb.AppendLine();

    sb.AppendLine("opt = Optimize()");
    sb.AppendLine();

    foreach (var b in machine.Buttons)
    {
        var v = ToLabel(b);
        sb.AppendLine($"{v} = Int('{v}')"); // p0 = Int('p0')
    }

    sb.AppendLine("cost = Int('cost')");
    sb.AppendLine();

    foreach (var b in machine.Buttons)
    {
        sb.AppendLine($"opt.add({ToLabel(b)} >= 0)"); // opt.add(p0 >= 0)
    }

    // cost = Int('cost')
    // opt.add(cost == p0 + p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9 + p10 + p11 + p12)
    sb.AppendLine($"opt.add(cost == {String.Join(" + ", machine.Buttons.Select(ToLabel))})");
    sb.AppendLine();

    for (int j = 0; j < machine.Joltages.Length; ++j)
    {
        var activeButtons = machine.Buttons.Where(b => b.Vector[j] == 1);
        if (activeButtons.Any())
        {
            // opt.add(p4 + p5 + p6 + p9 + p10 + p11 == 86)
            sb.AppendLine($"opt.add({String.Join(" + ", activeButtons.Select(ToLabel))} == {machine.Joltages[j]})");
        }
    }

    sb.AppendLine();
    sb.AppendLine("h = opt.minimize(cost)");
    sb.AppendLine("opt.check()");
    sb.AppendLine("print(opt.lower(h))");

    return sb.ToString();

    string ToLabel(Button b) => $"p{b.ButtonIndex}";
}

static string ExecPy(string script)
{
    string scriptPath = "script.py";
    File.WriteAllText(scriptPath, script);
    var startInfo = new ProcessStartInfo
    {
        FileName = PyPath,
        Arguments = scriptPath,
        UseShellExecute = false,
        RedirectStandardOutput = true,
    };
    using var process = Process.Start(startInfo);
    using StreamReader reader = process.StandardOutput;
    string result = reader.ReadToEnd();
    return result;
}

record Button(int ButtonIndex, int[] JoltIndexes, int JoltCount) : IEquatable<Button>
{
    public int Count => JoltIndexes.Length;
    public int Mask => JoltIndexes.Aggregate(0, (m, i) => m | (1 << i));
    
    public int[] Vector => _vector;
    private readonly int[] _vector = Enumerable.Range(0, JoltCount).Select(i => JoltIndexes.Contains(i) ? 1 : 0).ToArray();

    public int MaxPushes(int[] jolts) => Vector
        .EquiZip(jolts, (i, j) => (Increment:i, Joltage:j))
        .Where(v => v.Increment > 0)
        .Min(v => v.Increment * v.Joltage);

    public virtual bool Equals(Button? other) =>
        other != null &&
        ButtonIndex == other.ButtonIndex &&
        JoltIndexes.SequenceEqual(other.JoltIndexes);

    public override int GetHashCode() =>
        JoltIndexes.Aggregate(ButtonIndex, (hash, j) => unchecked(hash * 31 + j));

    public override string ToString() => $"B{ButtonIndex}";
}

record Machine(bool[] Indicators, int[] Joltages, List<Button> Buttons) : IEquatable<Machine>
{
    public static Machine Parse(string line)
    {
        // e.g. "[.##.] (3) (1,3) (2) (2,3) (0,2) (0,1) {3,5,4,7}"

        List<string> a = line.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();

        bool[] indicators = a[0].Trim('[', ']').Select(c => c == '#').ToArray();
        int[] joltages = a[^1].Trim('{', '}').Split(",").Select(int.Parse).ToArray();
        List<Button> buttons = a[1..^1]
            .Select(s => s.Trim('(', ')').Split(',').Select(int.Parse).ToArray())
            .Select((bs, i) => new Button(i, bs, joltages.Length))
            .ToList();

        Debug.Assert(indicators.Length == joltages.Length);

        return new Machine(indicators, joltages, buttons);
    }

    public int IndicatorMask => Indicators.ToBitMask();
    public int JoltageMask(int bit) => Joltages.ToBitMask(bit);

    public virtual bool Equals(Machine? other) =>
        other != null &&
        Joltages.SequenceEqual(other.Joltages) &&
        Indicators.SequenceEqual(other.Indicators) &&
        Buttons.SequenceEqual(other.Buttons);

    public override int GetHashCode() =>
        Joltages
        .Concat(Indicators.Select(i => i ? 1 : 0))
        .Concat(Buttons.Select(b => b.GetHashCode()))
        .Aggregate(17, (hash, val) => unchecked(hash * 31 + val));

    public override string ToString() => $"[{Indicators.ToIndicatorString()}] {String.Join(' ', Buttons.Select(b => $"({string.Join(',', b.JoltIndexes)})"))} {{{string.Join(',', Joltages)}}}";
}

static class Extensions
{
    public static int ToBitMask(this bool[] indicators) =>
        Enumerable.Range(0, indicators.Length).Where(i => indicators[i]).ToBitMask();

    public static int ToBitMask(this int[] values, int bit) =>
        Enumerable.Range(0, values.Length).Where(i => (values[i] & (1 << bit)) != 0).ToBitMask();

    private static int ToBitMask(this IEnumerable<int> bits) =>
        bits.Aggregate(0, (mask, bit) => mask | (1 << bit));

    public static string ToIndicatorString(this int mask, int bits) =>
        MoreEnumerable.Sequence(bits - 1, 0).Select(bit => (mask & (1 << bit)) != 0).ToIndicatorString();

    public static string ToIndicatorString(this IEnumerable<bool> indicators) =>
        String.Concat(indicators.Select(b => b ? '#' : '.'));

    public static int[] ZipAssign(this int[] accumulator, IEnumerable<int> operand, Func<int, int, int> op)
    {
        var a = operand.GetEnumerator();
        for (int i = 0; i < accumulator.Length; ++i)
        {
            if (!a.MoveNext())
                throw new IndexOutOfRangeException();

            accumulator[i] = op(accumulator[i], a.Current);
        }
        return accumulator;
    }

    public static IEnumerable<int> ToPushCounts(this IEnumerable<(Button Button, int Pushes)> buttonPushes, int buttonCount)
    {
        var pushesByButtonIndex = buttonPushes.ToLookup(b => b.Button.ButtonIndex, b => b.Pushes);
        return Enumerable.Range(0, buttonCount).Select(i => pushesByButtonIndex[i].Sum());
    }

    public static IEnumerable<T[]> OrderedCombinations<T>(this IEnumerable<T> source)
    {
        var items = source.ToList();
        return Enumerable.Range(0, items.Count + 1).SelectMany(k => Generate(0, items.Count, k));

        // generate all n select k combinations starting at index h
        IEnumerable<T[]> Generate(int h, int n, int k) =>
            k == 0 ? [[]] : // single zero-length combination
            Enumerable.Range(0, n - k + 1).SelectMany(i =>
                Generate(h + i + 1, n - (i + 1), k - 1)
                .Select(tail => tail.Prepend(items[h + i]).ToArray()));
    }
}