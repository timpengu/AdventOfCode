
List<int> program =
    String.Concat(File.ReadLines("input.txt"))
    .Split(',', StringSplitOptions.RemoveEmptyEntries)
    .Select(int.Parse)
    .ToList();

// part 1
Execute(program, 1);

// part 2
Execute(program, 5);

void Execute(IEnumerable<int> program, params int[] inputs)
{
    var computer = new Computer(program, inputs);
    Console.WriteLine($"\nInput:  [{String.Join(',', computer.Inputs)}]");
    computer.Execute();
    Console.WriteLine($"Output: [{String.Join(',', computer.Outputs)}]");
}
