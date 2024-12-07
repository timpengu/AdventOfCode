internal static class Program
{
    public static bool Verbose { get; } = true;

    record struct Operator(string Symbol, Func<long, long, long> Apply)
    {
        public override string ToString() => Symbol;
    }

    static readonly Operator Add = new("+", (a, b) => a + b);
    static readonly Operator Multiply = new("*", (a, b) => a * b);
    static readonly Operator Concat = new("||", (a, b) => long.Parse($"{a}{b}")); // performance lolz

    public static void Main(string[] args)
    {
        var input = new List<(long Target, long[] Operands)>(
            File.ReadLines("input.txt").Select(line =>
            {
                string[] targetAndOperands = line.Split(':', 2);
                return (
                    long.Parse(targetAndOperands[0]),
                    targetAndOperands[1].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToArray()
                );
            }));

        long result1 = input.SolveWith(Add, Multiply);
        long result2 = input.SolveWith(Add, Multiply, Concat);

        Console.WriteLine($"\nTotal result (with +,*): {result1}");
        Console.WriteLine($"\nTotal result (with +,*,||): {result2}");
    }

    private static long SolveWith(this IEnumerable<(long Target, long[] Operands)> input, params Operator[] operatorSet)
    {
        var solver = new Solver(operatorSet);
        var results = input
            .Select(item => (
                Item: item,
                Solutions: solver.Find(item.Target, item.Operands).ToList()
            ));

        if (Verbose)
        {
            foreach (var result in results)
            {
                Console.WriteLine($"\n{result.Item.Target}: {String.Join(" ", result.Item.Operands)}");
                foreach (var operators in result.Solutions)
                {
                    Console.WriteLine(String.Join(" ", operators));
                }
                if (result.Solutions.Count == 0)
                {
                    Console.WriteLine("No solution");
                }
            }
        }

        return results.Where(r => r.Solutions.Any()).Sum(r => r.Item.Target);
    }

    private class Solver
    {
        public IReadOnlyCollection<Operator> OperatorSet { get; }
        public Solver(params Operator[] operatorSet)
        {
            OperatorSet = operatorSet;
        }

        public IEnumerable<Operator[]> Find(long target, IEnumerable<long> operands) => Find(target, operands.First(), operands.Skip(1), Enumerable.Empty<Operator>());
        public IEnumerable<Operator[]> Find(long target, long accumulator, IEnumerable<long> operands, IEnumerable<Operator> operators)
        {
            if (!operands.Any()) // no more operands to recurse
            {
                return accumulator == target
                    ? SingleSolution(operators)
                    : NoSolution();
            }

            return OperatorSet.SelectMany(op => Find(
                target,
                op.Apply(accumulator, operands.First()),
                operands.Skip(1),
                operators.Append(op)
            ));
        }

        private static IEnumerable<Operator[]> SingleSolution(IEnumerable<Operator> operators) => Enumerable.Repeat(operators.ToArray(), 1);
        private static IEnumerable<Operator[]> NoSolution() => Enumerable.Empty<Operator[]>();
    }
}
