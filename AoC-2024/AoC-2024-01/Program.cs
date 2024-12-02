
List<int[]> ls = new(
    File.ReadLines("input.txt")
        .Select(line => line
            .Split(default(char[]), 2, StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToArray()       
    ));

var xs = ls.Select(l => l[0]);
var ys = ls.Select(l => l[1]);

int distance = xs.Order().Zip(ys.Order(), (x, y) => Math.Abs(x - y)).Sum();
int similarity = xs.Sum(x => x * ys.Count(y => x == y));

Console.WriteLine($"{distance} {similarity}");
