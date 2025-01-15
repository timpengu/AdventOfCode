using MoreLinq;
using IntCode;

internal static class Program
{
    private static void Main(string[] args)
    {
        List<int> program =
            string.Concat(File.ReadLines("input.txt"))
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList();

        (IList<int> Phases, int Signal) FindMaxSignal(IEnumerable<int> phases, Func<IList<int>, IList<int>, int> runAmps)
        {
            return phases
                .Permutations()
                .Select(phasesPermuted => (Phases: phasesPermuted, Signal: runAmps(program, phasesPermuted)))
                .MaxBy(x => x.Signal);
        }

        var part1 = FindMaxSignal(Enumerable.Range(0, 5), RunAmpsOnePass);
        Console.WriteLine($"\nMax signal: {part1.Signal} (one pass with phase sequence {string.Join(',', part1.Phases)})");

        var part2 = FindMaxSignal(Enumerable.Range(5, 5), RunAmpsFeedback);
        Console.WriteLine($"\nMax signal: {part2.Signal} (feedback with phase sequence {string.Join(',', part2.Phases)})");
    }

    private static int RunAmpsOnePass(IList<int> program, IList<int> phases)
    {
        Console.WriteLine($"\nPhase sequence: {string.Join(',', phases)}");

        int signal = 0;
        foreach (int phase in phases)
        {
            int[] inputs = [phase, signal];
            Console.WriteLine($"Input:  [{string.Join(',', inputs)}]");

            var computer = new Computer<int>(program, inputs);
            signal = computer.ExecuteOutputs().First();
            Console.WriteLine($"Output: {signal}");
        }
        
        return signal;
    }

    private static int RunAmpsFeedback(IList<int> program, IList<int> phases)
    {
        Console.WriteLine($"\nPhase sequence: {string.Join(',', phases)}");

        List<(char Id, Computer<int> Computer, InputQueue<int> InputQueue)> amps = new(
            phases.Select((phase, index) =>
            {
                char id = (char)('A' + index);
                var inputQueue = new InputQueue<int>([phase]);
                var computer = new Computer<int>(program, inputQueue);
                return (id, computer, inputQueue);
            })
        );

        int signal = 0;
        while(true)
        {
            foreach (var amp in amps)
            {
                amp.InputQueue.Enqueue(signal);
                signal = amp.Computer.ExecuteOutputs().FirstOrDefault(signal);
                if (amp.Computer.IsHalted)
                {
                    Console.WriteLine($"Halted {amp.Id}!");
                    return signal;
                }
                Console.WriteLine($"Output {amp.Id}: {signal}");
            }
        }
    }
}
