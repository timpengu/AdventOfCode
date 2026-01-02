using System.Reactive.Linq;
using System.Threading;

const int NicCount = 50;

long[] program =
    String.Concat(File.ReadLines("input.txt"))
    .Split(',', StringSplitOptions.RemoveEmptyEntries)
    .Select(long.Parse)
    .ToArray();

IReadOnlyCollection<Nic> nics =
    Enumerable.Range(0, NicCount)
    .Select(id => new Nic(program, id))
    .ToList();

Nat nat = new(
    NodeIds.Nat,
    NodeIds.Primary,
    () => nics.All(nic => nic.IsIdle));

IReadOnlyCollection<INode> allNodes = nics.Append<INode>(nat).ToList();
Router router = new(allNodes);

using var routingSubscription = allNodes
    .Select(source => source.ToSentPacketObservable())
    .Merge()
    .Subscribe(e => router.GetDestination(e.Packet)?.ReceivePacket(e.Packet));

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine($"Starting nodes (with {nics.Count} NICs)");

var cancellationSource = new CancellationTokenSource();
var tasks = allNodes
    .Select(node => node.ExecuteAsync(cancellationSource.Token))
    .ToArray();

await Task.WhenAny(tasks);

try
{
    cancellationSource.Cancel();
    await Task.WhenAll(tasks);
}
catch (OperationCanceledException)
{
}

Console.WriteLine("All nodes stopped");
