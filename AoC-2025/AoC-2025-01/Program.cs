
List<int> rotations =
    File.ReadLines("input.txt")
    .Select(s => s.Trim().Replace('R', '+').Replace('L', '-'))
    .Select(int.Parse)
    .ToList();

int dial = 50;
int range = 100;
int z1 = 0;
int z2 = 0;

Console.WriteLine($"{dial}");

foreach (int rot in rotations)
{
    int dz2 = rot >= 0
        ? (dial + rot) / range
        : (Inv(dial, range) - rot) / range;

    dial = Mod(dial + rot, range);

    int dz1 = dial == 0 ? 1 : 0;

    z1 += dz1;
    z2 += dz2;

    Console.WriteLine($"{rot:+0;-0} => {dial}, dz1={dz1}, dz2={dz2}");
}

Console.WriteLine($"\nz1={z1}, z2={z2}");

static int Inv(int x, int m) => Mod(m - x, m);
static int Mod(int x, int m) => (x % m + m) % m;
