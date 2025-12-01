
foreach (string line in File.ReadLines("input.txt"))
{
    var values = line.Trim().Select(ToDigit).ToArray();
    if (values.Length == 0)
    {
        Console.WriteLine();
        continue;
    }

    int len2 = values.Length / 2;
    int sum1 = SumMatching(values, 1);
    int sum2 = SumMatching(values, len2);

    Console.WriteLine($"{String.Concat(values)}: len={values.Length} sum(1)={sum1} sum({len2})={sum2}");
}

static int ToDigit(char c) =>
    c >= '0' && c <= '9'
    ? c - '0'
    : throw new Exception($"Invalid digit: {c}");

static int SumMatching(int[] values, int offset)
{
    bool IsMatchingIndex(int index) => values[index] == values[(index + offset) % values.Length];
    return Enumerable.Range(0, values.Length).Where(IsMatchingIndex).Sum(i => values[i]);
}
