
List<int[]> ls = new(
    File.ReadLines("input.txt")
        .Select(line => line
            .Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToArray()
    ));

int cksum1 = ls.Sum(MinMaxDifference);
Console.WriteLine(cksum1);

int cksum2 = ls.Sum(DivisorRatio);
Console.WriteLine(cksum2);
static int MinMaxDifference(int[] values) => values.Max() - values.Min();

static int DivisorRatio(int[] values)
{
    for(int i = 0; i < values.Length; ++i)
    {
        for (int j = 0; j < values.Length; ++j)
        {
            if (i != j && values[i] % values[j] == 0)
            {
                return values[i] / values[j];
            }
        }
    }

    throw new Exception("No dividing pair found");
}
