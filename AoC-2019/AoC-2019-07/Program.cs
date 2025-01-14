using MoreLinq;

List<int> program =
    String.Concat(File.ReadLines("input.txt"))
    .Split(',', StringSplitOptions.RemoveEmptyEntries)
    .Select(int.Parse)
    .ToList();

var maxSignal =
    Enumerable.Range(0, 5)
    .Permutations()
    .Select(phases => (Phases: phases, Signal: RunAmps(phases)))
    .MaxBy(x => x.Signal);

Console.WriteLine($"\nMax signal: {maxSignal.Signal} (with phase sequence {String.Join(',', maxSignal.Phases)})");

int RunAmps(IList<int> phases)
{
    Console.WriteLine($"\nPhase sequence: {String.Join(',', phases)}");
    return phases.Aggregate(0, (signal, phase) => RunAmp(phase, signal));
}

int RunAmp(int phase, int input)
{
    var computer = new Computer(program, [phase, input]);
    Console.WriteLine($"Input:  [{String.Join(',', computer.Inputs)}]");
    computer.Execute();
    Console.WriteLine($"Output: [{String.Join(',', computer.Outputs)}]");
    return computer.Outputs.Single();
}