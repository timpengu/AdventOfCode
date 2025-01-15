using System.Numerics;

namespace IntCode;

public static class ComputerExtensions
{
    public static void ExecuteAll<T>(this Computer<T> computer)
        where T : struct, INumber<T>, ISignedNumber<T>
    {
        while (!computer.IsHalted)
        {
            computer.ExecuteOne();
        }
    }

    public static IEnumerable<T> ExecuteOutputs<T>(this Computer<T> computer)
        where T : struct, INumber<T>, ISignedNumber<T>
    {
        while (!computer.IsHalted)
        {
            computer.ExecuteOne();

            foreach (var output in computer.GetOutputs())
            {
                yield return output;
            }
        }
    }
}