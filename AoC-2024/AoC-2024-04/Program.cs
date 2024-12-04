public static class Program
{
    private static void Main(string[] args)
    {
        Grid grid = new(File.ReadLines("input.txt"));

        Coord[] allDirections = { (0, 1), (1, 0), (0, -1), (-1, 0), (1, 1), (1, -1), (-1, -1), (-1, 1) };
        List<CoordSpan> xmasSolutions = grid.FindWords("XMAS", allDirections).ToList();

        Console.WriteLine("\nXMAS solutions:");
        grid.WriteToConsole(xmasSolutions);

        Coord[] diagonalsForward = { (1, 1), (-1, -1) };
        Coord[] diagonalsBackward = { (1, -1), (-1, 1) };

        List<CoordSpan> masSolutionsForward = grid.FindWords("MAS", diagonalsForward).ToList();
        List<CoordSpan> masSolutionsBackward = grid.FindWords("MAS", diagonalsBackward).ToList();

        List<(CoordSpan Forward, CoordSpan Backward)> crossMasSolutions = masSolutionsForward.Join(masSolutionsBackward, fwd => fwd[1], bck => bck[1], (fwd, bck) => (fwd, bck)).ToList();

        Console.WriteLine("\nX-MAS solutions:");
        grid.WriteToConsole(crossMasSolutions.SelectMany(x => new[] { x.Forward, x.Backward }));

        Console.WriteLine($"\n{xmasSolutions.Count} {crossMasSolutions.Count}");
    }

    private static IEnumerable<CoordSpan> FindWords(this Grid grid, string word, params Coord[] directions) =>
        from z in grid.Range()
        from dz in directions
        let span = new CoordSpan(z, dz, word.Length)
        where grid.MatchesWord(span, word)
        select span;

    private static bool MatchesWord(this Grid grid, CoordSpan span, string word) => span.Zip(word, grid.MatchesChar).All(isMatch => isMatch);
    private static bool MatchesChar(this Grid grid, Coord coord, char c) => grid.IsInRange(coord) && grid[coord] == c;

    private static void WriteToConsole(this Grid grid, IEnumerable<CoordSpan> solutions)
    {
        ISet<Coord> solutionCoords = solutions.SelectMany(sol => sol).ToHashSet();

        for (int y = 0; y < grid.XSize; ++y)
        {
            for (int x = 0; x < grid.XSize; ++x)
            {
                Coord z = (x, y);
                Console.ForegroundColor = solutionCoords.Contains(z) ? ConsoleColor.White : ConsoleColor.DarkRed;
                Console.Write(grid[z]);
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
        }
    }
}
