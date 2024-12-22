
using MoreLinq;

IList<ulong> seeds = File.ReadLines("input.txt").Select(ulong.Parse).ToList();

ulong modulo = 16777216;
int iterations = 2000;

ulong total = 0;
foreach(ulong seed in seeds)
{
    ulong value = Generate(seed, iterations);
    total += value;

    Console.WriteLine($"{seed}: {value}");
}

Console.WriteLine($"Total of {seeds.Count} values after {iterations} iterations: {total}");

ulong Generate(ulong seed, int iterations)
{
    ulong value = seed;
    for (int i = 0; i < iterations; ++i)
    {
        value = GenerateNext(value);
    }
    return value;
}

ulong GenerateNext(ulong value)
{
    value = Mix(value, value << 6);
    value = Mix(value, value >> 5);
    value = Mix(value, value << 11);
    return value;
}

ulong Mix(ulong a, ulong b) => (a ^ b) % modulo;