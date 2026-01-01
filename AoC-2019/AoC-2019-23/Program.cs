using System.Reactive.Linq;
using static Nic;

internal static class Program
{
    public static long NatId = 255;

    public static async Task Main(string[] args)
    {
        long[] program =
            string.Concat(File.ReadLines("input.txt"))
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(long.Parse)
                .ToArray();

        List<Nic> nics = Enumerable.Range(0, 50)
            .Select(id => new Nic(program, id))
            .ToList();

        using var sentPacketsSubscription = nics
            .Select(nic => nic.ToSentPacketObservable())
            .Merge()
            .Subscribe(e => nics.RoutePacket(e.Packet));

        Console.WriteLine($"Starting {nics.Count} NICs");

        await Task.WhenAll(nics.Select(nic => nic.ExecuteAsync()));

        Console.WriteLine("All NICs stopped");
    }

    private static void RoutePacket(this IReadOnlyList<Nic> nics, Packet packet)
    {
        int id = packet.DestinationId;
        if (id >= 0 && id < nics.Count)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{packet}");

            nics[id].ReceivePacket(packet);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{packet} unrouteable");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    private static IObservable<SentPacketEventArgs> ToSentPacketObservable(this Nic nic) =>
        Observable.FromEvent<EventHandler<SentPacketEventArgs>, SentPacketEventArgs>(
            action => (object? sender, SentPacketEventArgs e) => action(e),
            handler => nic.SentPacket += handler,
            handler => nic.SentPacket -= handler);
}
