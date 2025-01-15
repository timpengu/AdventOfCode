using IntCode;

List<int> program =
    String.Concat(File.ReadLines("input.txt"))
    .Split(',', StringSplitOptions.RemoveEmptyEntries)
    .Select(int.Parse)
    .ToList();

// part 1
Execute(program, 1);

// part 2
Execute(program, 5);

void Execute(IEnumerable<int> program, params int[] input)
{
    var computer = new Computer<int>(program, input);
    Console.WriteLine($"\nInput:  [{String.Join(',', input)}]");
    var output = computer.ExecuteOutputs().ToList();
    Console.WriteLine($"Output: [{String.Join(',', output)}]");
}
