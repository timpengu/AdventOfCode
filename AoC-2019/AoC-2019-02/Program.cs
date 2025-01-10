
List<int> program =
    File.ReadLines("input.txt")
    .Single()
    .Split(',', StringSplitOptions.RemoveEmptyEntries)
    .Select(int.Parse)
    .ToList();

Part1();

int output = 19690720;
Console.WriteLine($"\nSearching for: {output}");
int input = Part2(output).ToList().Single();
Console.WriteLine($"Successful input: {input}");

void Part1()
{
    var computer = new Computer(program, verbose: true);
    computer[1] = 12;
    computer[2] = 02;
    computer.Execute();
    Console.WriteLine($"\nHalted with [0] = {computer[0]}");
}

IEnumerable<int> Part2(int searchOutput)
{
    for (int noun = 0; noun <= 99; ++noun)
    {
        for (int verb = 0; verb <= 99; ++verb)
        {
            var computer = new Computer(program);
            computer[1] = noun;
            computer[2] = verb;
            computer.Execute();

            int input = 100 * noun + verb;
            int output = computer[0];
            Console.WriteLine($"{input:0000} => {output}");

            if (output == searchOutput)
            {
                yield return input;
            }
        }
    }
}
