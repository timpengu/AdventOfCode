using MoreLinq;
using MoreLinq.Extensions;
using System.Text.RegularExpressions;

static class InputExtensions
{
    // TOOD: rewrite as IEnumerable<string> parser
    public static IEnumerable<Tile> ParseTiles(this IEnumerable<string> lines)
    {
        IEnumerator<string> lineEnumerator = lines.GetEnumerator();
        while (lineEnumerator.MoveNext())
        {
            string line = lineEnumerator.Current;
            if (line.Length == 0)
            {
                // skip blank lines before tile header
                continue;
            }

            // read tile header
            Match match = Regex.Match(line ?? String.Empty, @"^Tile ([0-9]+):$");
            if (!match.Success || !int.TryParse(match.Groups[1].Value, out int id))
            {
                throw new Exception($"Cannot parse tile header: '{line}'");
            }

            // read tile image
            List<string> tile = new();
            while (lineEnumerator.MoveNext() && lineEnumerator.Current.Length > 0)
            {
                tile.Add(lineEnumerator.Current);
            }
            
            // build tile
            int ys = tile.Count;
            int xs = tile.Select(p => p.Length).Distinct().Single();
            bool[,] image = new bool[xs, ys];
            for (int y = 0; y < ys; ++y)
            {
                for (int x = 0; x < xs; ++x)
                {
                    char c = tile[y][x];
                    bool isSet = c switch
                    {
                        '#' => true,
                        '.' => false,
                        _ => throw new Exception($"Invalid image char '{c}'")
                    };

                    image[x, y] = isSet;
                }
            }

            yield return new Tile(id, image);
        }
    }

    public static IEnumerable<Coord> ParsePatternCoords(this IEnumerable<string> lines)
    {
        IEnumerator<string> yEnumerator = lines.GetEnumerator();
        for (int y = 0; yEnumerator.MoveNext(); ++y)
        {
            string line = yEnumerator.Current;
            for (int x = 0; x < line.Length; ++x)
            {
                char c = line[x];
                bool isRequired = c switch
                {
                    '#' => true,
                    ' ' => false,
                    _ => throw new Exception($"Invalid pattern char '{c}'")
                };

                if (isRequired)
                {
                    yield return new Coord(x, y);
                }
            }
        }
    }
}