using IntCode;

internal static class Program
{
    private static readonly int[] _program =
        string.Concat(File.ReadLines("input.txt"))
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToArray();

    public static void Main(string[] args)
    {
        int sum = Part1(0, 0, 50, 50);
        
        (int x, int y) = Part2(0, 20, 10, 100);
        int value = x * 10000 + y;
        Console.WriteLine($"Value = {value}");
    }

    private static int Part1(int x0, int y0, int xs, int ys)
    {
        int sum = 0;
        foreach(int y in Enumerable.Range(y0,ys))
        {
            foreach (int x in Enumerable.Range(x0, xs))
            {
                bool isTractor = IsInBeam(x, y);
                Console.Write(isTractor ? '#' : '.');
                sum += isTractor ? 1 : 0;
            }
            Console.WriteLine();
        }

        Console.WriteLine($"Beam area in {xs} x {ys} = {sum}");
        return sum;
    }

    private static (int X, int Y) Part2(int xMin, int xMax, int y0, int size)
    {
        for (int y = y0; ; ++y)
        {
            xMin = FindBeamXMin(xMin + 1, y);
            xMax = FindBeamXMax(xMax + 1, y);

            // Console.WriteLine($"y={y}: x={xMin}..{xMax}");

            for(int x = xMax - size; x + size - 1 <= xMax; ++x)
            {
                if (x >= xMin && IsSquareInBeam(x, y, size))
                {
                    return (x, y);
                }
            }
        }
    }

    private static int FindBeamXMin(int x, int y)
    {
        while (IsInBeam(x, y)) --x;
        while (!IsInBeam(x, y)) ++x;
        return x;
    }

    private static int FindBeamXMax(int x, int y)
    {
        while (IsInBeam(x, y)) ++x;
        while (!IsInBeam(x, y)) --x;
        return x;
    }

    private static bool IsSquareInBeam(int x, int y, int size)
    {
        int xc = x + size - 1;
        int yc = y + size - 1;
        if (IsInBeam(x, y) &&
            IsInBeam(xc, y) &&
            IsInBeam(x, yc) &&
            IsInBeam(xc, yc))
        {
            Console.WriteLine($"({x},{y}) is in beam");
            Console.WriteLine($"({xc},{y}) is in beam");
            Console.WriteLine($"({x},{yc}) is in beam");
            Console.WriteLine($"({xc},{yc}) is in beam");
            return true;
        }
        return false;
    }

    private static bool IsInBeam(int x, int y)
    {
        int output = new Computer<int>(_program, x, y).ExecuteOutputs().Single();
        return output switch
        {
            0 => false,
            1 => true,
            _ => throw new Exception($"Unexpected output: {output}")
        };
    }
}