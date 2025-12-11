using MoreLinq;
using System.Collections.Immutable;
using System.Diagnostics;

const string InputFile = "input.txt";
const string CacheFile = $"solutions.{InputFile}";

bool isEasy = false;

List<Machine> machines = File.ReadLines(InputFile)
    .Where(s => !s.Trim().StartsWith("//"))
    .Select(Machine.Parse)
    .ToList();

int indicatorTotal = 0;
int joltageTotal = 0;

for (int m = 0; m < machines.Count; m++)
{
    var machine = machines[m];
    Console.WriteLine($"\n{m+1}/{machines.Count}: {machine}");

    List<int> indicatorSeq = FindIndicatorSequences(machine).First();
    indicatorTotal += indicatorSeq.Count;
    Console.WriteLine($"Indicator sequence: {String.Join(",", indicatorSeq)}");

    if (isEasy)
    {
        Stopwatch sw = Stopwatch.StartNew();
        List<int> joltagePushes = FindJoltageButtonCounts2(machine).First();
        joltageTotal += joltagePushes.Sum();
        Console.WriteLine($"Joltage sequence ({joltagePushes.Sum()}): {String.Join(",", joltagePushes)} [{sw.Elapsed}]");
    }
}

Console.WriteLine($"\nIndicator total: {indicatorTotal}");
Console.WriteLine();

if (!isEasy)
{
    var joltageSolutions = LoadFile(CacheFile);
    foreach (var kvp in joltageSolutions)
    {
        Console.WriteLine($"Loaded: {kvp.Key} => {String.Join(",", kvp.Value)}");
    }

    Dictionary<Machine, int> matches = machines
        .Select(m => (Machine: m, Matches: joltageSolutions.Keys.Count(k => IsMatch(m, k))))
        .Where(m => m.Matches > 0)
        .ToDictionary();

    Debug.Assert(matches.All(m => m.Value == 1));

    Stopwatch sw = Stopwatch.StartNew();
    int count = joltageSolutions.Count;
    int joltageSum = machines
        .Where(m => !matches.ContainsKey(m))
        .AsParallel()
        .Select((machine, i) =>
        {
            List<int> pushes = FindJoltageButtonCounts2(machine).First();
            lock (sw)
            {
                Console.WriteLine($"\n{++count}/{machines.Count} [{i + 1}]: {machine}");
                Console.WriteLine($"Pushes ({pushes.Sum()}): {String.Join(",", pushes)} [{sw.Elapsed}]");

                joltageSolutions[machine] = pushes;
                AppendFile(machine, pushes, CacheFile);
            }
            return pushes.Sum();
        })
        .Sum();

    SaveFile(joltageSolutions, CacheFile);
    joltageTotal = joltageSolutions.Sum(j => j.Value.Sum());
}

Console.WriteLine($"\nJoltage total: {joltageTotal}");

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

IEnumerable<List<int>> FindJoltageButtonSequences(Machine machine)
{
    // Reorder buttons heuristically
    List<Button> buttons = machine.Buttons
        .OrderByDescending(b => b.JoltIndexes.Length)
        .ToList();

    var initialState = (machine.Joltages, Sequence: Enumerable.Empty<int>());
    Queue<(int[] Joltages, IEnumerable<int> Sequence)> queue = new([initialState]);
    while (queue.TryDequeue(out var state))
    {
        if (state.Joltages.Any(j => j < 0))
            continue;

        //Console.WriteLine($"{{{String.Join(",", state.Joltages)}}} => {String.Join(", ", state.Sequence)}");

        if (state.Joltages.All(j => j == 0))
        {
            yield return state.Sequence.ToList();
        }
        else
        {
            foreach (var button in buttons)
            {
                var nextState = (
                    Joltages: state.Joltages.Zip(button.Vector, (j, b) => j - b).ToArray(),
                    Sequence: state.Sequence.Append(button.ButtonIndex)
                );
                queue.Enqueue(nextState);
            }
        }
    }
}

IEnumerable<List<int>> FindJoltageButtonCounts(Machine machine)
{
    // Reorder buttons heuristically
    List<Button> buttons = machine.Buttons
        .OrderByDescending(b => b.JoltIndexes.Length)
        .ThenByDescending(b => b.MaxPushes(machine.Joltages))
        .ToList();

    List<int> GetResult(IEnumerable<int> buttonPushes)
    {
        // Map button pushes back to original button order
        var result = new int[buttons.Count];
        var ps = buttonPushes.Select((p, i) => (buttons[i].ButtonIndex, Pushes: p));
        foreach (var p in ps)
        {
            result[p.ButtonIndex] = p.Pushes;
        }
        return result.ToList();
    }

    return Solve(machine.Joltages, [], 0);

    IEnumerable<List<int>> Solve(int[] joltages, IEnumerable<int> buttonPushes, int buttonIndex)
    {
        if (buttonIndex == buttons.Count)
        {
            if (joltages.All(j => j == 0))
            {
                yield return GetResult(buttonPushes);
            }
        }
        else
        {
            var button = buttons[buttonIndex];
            int maxPushes = button.MaxPushes(joltages);
            int minPushes = buttonIndex == buttons.Count - 1 ? maxPushes : 0;

            for (int pushes = maxPushes; pushes >= minPushes; --pushes)
            {
                int[] nextJoltages = joltages.EquiZip(button.Vector, (j, b) => j - pushes * b).ToArray();
                var solutions = Solve(nextJoltages, buttonPushes.Append(pushes), buttonIndex + 1);
                foreach (var solution in solutions)
                {
                    yield return solution;
                }
            }
        }
    }
}

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

    var zeroPushes = ImmutableArray<int>.Empty.AddRange(Enumerable.Repeat(0, machine.Buttons.Count));
    return Solve(machine.Joltages, zeroPushes, 0, 0);

    IEnumerable<List<int>> Solve(int[] joltages, ImmutableArray<int> buttonPushes, int j, int b)
    {
        if (j == joltIndexes.Length)
        {
            if (joltages.All(j => j == 0))
            {
                yield return buttonPushes.ToList();
            }
        }
        else
        {
            // Console.WriteLine($"{{{String.Join(",", joltages)}}} => {String.Join(",", buttonPushes)}");

            int joltIndex = joltIndexes[j];
            var buttons = buttonsByJoltIndex[joltIndex];
            bool isLastButton = b == buttons.Length - 1;
            (int jNext, int bNext) = isLastButton ? (j + 1, 0) : (j, b + 1);

            var button = buttons[b];
            int minPushes = isLastButton ? joltages[joltIndex] : 0;
            int maxPushes = button.MaxPushes(joltages);

            for (int pushes = maxPushes; pushes >= minPushes; --pushes) // if any
            {
                (int[] nextJoltages, var nextButtonPushes) = pushes == 0
                    ? (joltages, buttonPushes)
                    : (
                        joltages.EquiZip(button.Vector, (j, b) => j - pushes * b).ToArray(),
                        buttonPushes.SetItem(button.ButtonIndex, buttonPushes[button.ButtonIndex] + pushes)
                    );
                
                var solutions = Solve(nextJoltages, nextButtonPushes, jNext, bNext);
                foreach(var solution in solutions)
                {
                    yield return solution;
                }
            }
        }
    }
}


static bool IsMatch(Machine a, Machine b)
{
    return
        a.Joltages.SequenceEqual(b.Joltages) &&
        a.Indicators.SequenceEqual(b.Indicators) &&
        a.Buttons.Count == b.Buttons.Count;
}

static IDictionary<Machine, List<int>> LoadFile(string path)
{
    if (!File.Exists(path))
    {
        return new Dictionary<Machine, List<int>>();
    }

    using (var sr = new StreamReader(path))
    {
        Dictionary<Machine, List<int>> joltageSolutions = new();
        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine()!;
            var parts = line.Split("|", 2, StringSplitOptions.TrimEntries);
            Machine machine = Machine.Parse(parts[0]);
            List<int> pushes = parts[1].Split(",", StringSplitOptions.TrimEntries).Select(int.Parse).ToList();
            joltageSolutions[machine] = pushes;
        }
        return joltageSolutions;
    }
}

static void SaveFile(IDictionary<Machine, List<int>> solutions, string path)
{
    using (var sw = new StreamWriter(path))
    {
        foreach (var kvp in solutions)
        {
            Write(sw, kvp.Key, kvp.Value);
        }
    }
}

static void AppendFile(Machine machine, List<int> solution, string path)
{
    using (var sw = new StreamWriter(path,true))
    {
        Write(sw, machine, solution);
    }
}

static void Write(StreamWriter sw, Machine machine, List<int> solution)
{
    sw.WriteLine($"{machine} | {String.Join(',', solution)}");
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

    public int IndicatorMask =>
        Enumerable.Range(0, Indicators.Length)
        .Where(i => Indicators[i])
        .Aggregate(0, (mask, bit) => mask | (1 << bit));

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

    public override string ToString() => $"[{String.Concat(Indicators.Select(b => b ? '#' : '.'))}] {String.Join(' ', Buttons.Select(b => $"({string.Join(',', b.JoltIndexes)})"))} {{{string.Join(',', Joltages)}}}";
}

