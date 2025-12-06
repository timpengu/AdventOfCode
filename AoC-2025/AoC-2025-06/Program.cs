
List<string> lines = File.ReadLines("input.txt").ToList();

// Part 1
{
    List<string[]> splits = lines
        .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        .ToList();

    string[] ops = splits[^1];
    List<long[]> values = splits[..^1].Select(s => s.Select(long.Parse).ToArray()).ToList();
    int count = values.Select(v => v.Length).Append(ops.Length).Distinct().Single();

    long total = 0;

    for (int i = 0; i < count; ++i)
    {
        string op = ops[i];
        List<long> operands = values.Select(v => v[i]).ToList();

        long value = op.Compute(operands);
        Console.WriteLine($"{String.Join($" {op} ", operands)} = {value}");

        total += value;
    }

    Console.WriteLine($"Part 1 total: {total}\n");
}

// Part 2
{
    int length = lines.Select(v => v.Length).Max();
    lines = lines.Select(l => l.PadRight(length, ' ')).ToList();

    long total = 0;
    string op = String.Empty;
    List<long> operands = [];

    for (int i = 0; i < length; ++i)
    {
        op += lines[^1][i].ToString().Trim();

        List<int> digits = lines[..^1].Select(l => l[i]).Where(IsDigit).Select(ToDigit).ToList();
        if (digits.Any())
        {
            long operand = digits.Aggregate(0L, (value, digit) => 10 * value + digit);
            operands.Add(operand);
        }

        if (!digits.Any() || i + 1 == length)
        {
            long value = op.Compute(operands);
            Console.WriteLine($"{String.Join($" {op} ", operands)} = {value}");

            total += value;
            op = String.Empty;
            operands.Clear();
        }
    }

    Console.WriteLine($"Part 2 total: {total}\n");
}

static bool IsDigit(char c) => c >= '0' && c <= '9';
static int ToDigit(char c) => IsDigit(c) ? c - '0' : throw new ArgumentOutOfRangeException(nameof(c), $"Invalid digit: '{c}'");

internal static class Extensions
{
    public static long Compute(this string op, IEnumerable<long> operands) => op switch
    {
        "+" => operands.Sum(),
        "*" => operands.Product(),
        _ => throw new NotSupportedException($"Unknown op: '{op}'")
    };

    public static long Product(this IEnumerable<long> values) => values.Aggregate((a, b) => a * b);
}
