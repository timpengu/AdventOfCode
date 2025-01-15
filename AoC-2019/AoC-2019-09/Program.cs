List<long> program =
    string.Concat(File.ReadLines("input.txt"))
    .Split(',', StringSplitOptions.RemoveEmptyEntries)
    .Select(long.Parse)
    .ToList();

Execute(1);
Execute(2);

void Execute(params long[] input)
{
    var computer = new Computer(program, input);
    var output = computer.ExecuteOutputs().ToList();
    Console.WriteLine(String.Join(',', output));
}
