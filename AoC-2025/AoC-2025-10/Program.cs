using System.Diagnostics;

List<Machine> machines = File.ReadLines("example.txt").Select(Machine.Parse).ToList();

var maxIndicators = machines.MaxBy(m => m.Indicators.Count());
var maxButtons = machines.MaxBy(m => m.Buttons.Count());

Console.WriteLine($"Max indicators: [{maxIndicators.Indicators.Length}] {maxIndicators}");
Console.WriteLine($"Max buttons: [{maxButtons.Buttons.Count}] {maxButtons}");

int indicatorTotal = 0;
int joltageTotal = 0;

for (int m = 0; m < machines.Count; m++)
{
    var machine = machines[m];
    Console.WriteLine($"\n{m+1}/{machines.Count}: {machine}");

    List<int> indicatorSeq = FindIndicatorSequences(machine).First();
    indicatorTotal += indicatorSeq.Count;
    Console.WriteLine($"Indicator sequence: {String.Join(",", indicatorSeq)}");

    Stopwatch sw = Stopwatch.StartNew();
    List<int> joltageSeq = FindJoltageSequences(machine).First();
    joltageTotal += joltageSeq.Count;
    Console.WriteLine($"Joltage sequence: {String.Join(",", joltageSeq)} [{sw.Elapsed}]");
}

Console.WriteLine();
Console.WriteLine($"Indicator total: {indicatorTotal}");
Console.WriteLine($"Joltage total: {joltageTotal}");

IEnumerable<List<int>> FindIndicatorSequences(Machine machine)
{
    int targetIndicators = Enumerable.Range(0, machine.Indicators.Length).Where(i => machine.Indicators[i]).Aggregate(0, (m, v) => m | (1 << v));
    List<int> buttonMasks = machine.Buttons.Select(b => b.Aggregate(0, (m, v) => m | (1 << v))).ToList();

    var initialState = (
        Indicators: 0,
        Sequence: Enumerable.Empty<int>()
    );

    Queue<(int Indicators, IEnumerable<int> Sequence)> queue = new([initialState]);
    HashSet<int> visited = [];

    while (queue.TryDequeue(out var state))
    {
        if (visited.Add(state.Indicators))
        {
            // Console.WriteLine($" => {String.Join(",", state.Sequence)}");

            if (state.Indicators == targetIndicators)
            {
                yield return state.Sequence.ToList();
            }
            else
            {
                for (int i = 0; i < buttonMasks.Count; i++)
                {
                    var nextState = (
                        Indicators: state.Indicators ^ buttonMasks[i],
                        Sequence: state.Sequence.Append(i)
                    );
                    queue.Enqueue(nextState);
                }
            }
        }
    }
}

IEnumerable<List<int>> FindJoltageSequences(Machine machine)
{
    int[] targetJoltages = machine.Joltages;

    List<(int Index, bool[] Buttons)> buttons = machine.Buttons
        .Select((b,i) => (Buttons:b,Index:i))
        .OrderByDescending(b => b.Buttons.Length)
        .Select(b => (
            b.Index,
            Buttons: b.Buttons.Aggregate(new bool[targetJoltages.Length], (a, v) => { a[v] = true; return a; })
        ))
        .ToList();

    var initialState = (
        Joltages: Enumerable.Repeat(0, targetJoltages.Length).ToArray(),
        Sequence: Enumerable.Empty<int>()
    );

    Queue<(int[] Joltages, IEnumerable<int> Sequence)> queue = new([initialState]);

    while (queue.TryDequeue(out var state))
    {
        //Console.WriteLine($"{{{String.Join(",", state.Joltages)}}} => {String.Join(", ", state.Sequence)}");

        if (state.Joltages.SequenceEqual(targetJoltages))
        {
            yield return state.Sequence.ToList();
        }
        else if (state.Joltages.Zip(targetJoltages, (j, t) => (Joltage: j, Target: t)).All(v => v.Joltage <= v.Target))
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                var nextState = (
                    Joltages: state.Joltages.Zip(buttons[i].Buttons, (j, b) => b ? j + 1 : j).ToArray(),
                    Sequence: state.Sequence.Append(i)
                );
                queue.Enqueue(nextState);
            }
        }
    }
}

record Machine(bool[] Indicators, List<int[]> Buttons, int[] Joltages)
{
    public static Machine Parse(string line)
    {
        // e.g. "[.##.] (3) (1,3) (2) (2,3) (0,2) (0,1) {3,5,4,7}"

        List<string> a = line.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();

        bool[] indicators = a[0].Trim('[', ']').Select(c => c == '#').ToArray();
        int[] joltages = a[^1].Trim('{', '}').Split(",").Select(int.Parse).ToArray();
        List<int[]> buttons = a[1..^1].Select(s => s.Trim('(', ')').Split(",").Select(int.Parse).ToArray()).ToList();

        Debug.Assert(indicators.Length == joltages.Length);

        return new Machine(indicators, buttons, joltages);
    }

    public override string ToString() => $"[{String.Join("", Indicators.Select(b => b ? '#' : '.'))}] {String.Join(", ", Buttons.Select(b => $"({string.Join(",", b)})"))} {{{string.Join(", ", Joltages)}}}";
}

internal static class Extensions
{
}
