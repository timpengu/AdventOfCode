﻿using System.Text.RegularExpressions;

internal static class Program
{
    public static void Main(string[] args)
    {
        Coord bounds = (101, 103); // (11, 7) for inputSample.txt
        List<Input> inputs = new(File.ReadLines("input.txt").Select(Parse));

        Console.WriteLine($"With {inputs.Count} robots in {bounds} map\n");

        // Part 1
        int time = 100;
        List<Coord> positions = inputs.GetPositions(bounds, time).ToList();

        Coord centre = (bounds.X / 2, bounds.Y / 2);
        int q1 = positions.Count(z => z.X < centre.X && z.Y < centre.Y);
        int q2 = positions.Count(z => z.X > centre.X && z.Y < centre.Y);
        int q3 = positions.Count(z => z.X < centre.X && z.Y > centre.Y);
        int q4 = positions.Count(z => z.X > centre.X && z.Y > centre.Y);
        int factor = q1 * q2 * q3 * q4;

        ConsoleWriteMap2(positions, bounds);
        Console.WriteLine($"Time={time}: {q1} * {q2} * {q3} * {q4} = {factor}\n");

        // Part 2
        IEnumerable<(int Time, double StdDev)> interestingTimes =
            Enumerable.Range(0, 100000)
            .Select(time => (Time: time, StdDev: inputs.GetPositions(bounds, time).CalcStdDev()))
            .OrderBy(x => x.StdDev);

        foreach(var t in interestingTimes.Take(10))
        {
            ConsoleWriteMap2(inputs.GetPositions(bounds, t.Time), bounds);
            Console.WriteLine($"Time={t.Time}: StdDev={t.StdDev:f2}\n");
            
            Console.ReadLine();
        }
    }

    private static IEnumerable<Coord> GetPositions(this IEnumerable<Input> inputs, Coord size, int time) => inputs.Select(i => (i.z + time * i.dz) % size);

    private static double CalcStdDev(this IEnumerable<Coord> positions)
    {
        List<Coord> posList = positions.ToList();
        double meanX = posList.Average(p => p.X);
        double meanY = posList.Average(p => p.Y);

        double Sq(double x) => x * x;
        double stdev = Math.Sqrt(posList.Average(p =>
                Sq(p.X - meanX) + Sq(p.Y - meanY) // square of distance from mean
        ));

        return stdev;
    }

    private static void ConsoleWriteMap(IEnumerable<Coord> positions, Coord size)
    {
        Dictionary<Coord, int> positionCounts = positions.GroupBy(p => p).ToDictionary(g => g.Key, g => g.Count());

        foreach (int y in Enumerable.Range(0, size.Y))
        {
            foreach (int x in Enumerable.Range(0, size.X))
            {
                Coord z = (x, y);
                Console.Write(
                    positionCounts.TryGetValue(z, out int count)
                    ? count > 9 ? '*' : (char)('0' + count)
                    : '.');

            }
            Console.WriteLine();
        }
    }

    private static void ConsoleWriteMap2(IEnumerable<Coord> positions, Coord size)
    {
        HashSet<Coord> positionSet = positions.ToHashSet();

        for (int y = 0; y < size.Y; y += 2)
        {
            for (int x = 0; x < size.X; ++x)
            {
                Coord zUpper = (x, y);
                Coord zLower = (x, y + 1);

                Console.Write(
                    (positionSet.Contains(zUpper), positionSet.Contains(zLower)) switch
                    {
                        (false, false) => '.',
                        (true, false) => '\u2580',
                        (false, true) => '\u2584',
                        (true, true) => '\u2588'
                    });
            }
            Console.WriteLine();
        }
    }

    private record struct Input(Coord z, Coord dz);
    private static Input Parse(string line)
    {
        Match match = Regex.Match(line, @"^p=([+-]?[0-9]+),([+-]?[0-9]+) v=([+-]?[0-9]+),([+-]?[0-9]+)$");
        if (match.Success &&
            int.TryParse(match.Groups[1].Value, out int x) &&
            int.TryParse(match.Groups[2].Value, out int y) &&
            int.TryParse(match.Groups[3].Value, out int dx) &&
            int.TryParse(match.Groups[4].Value, out int dy))
        {
            return new Input((x, y), (dx, dy));
        }

        throw new FormatException($"Failed to parse line: '{line}'");
    }
}
