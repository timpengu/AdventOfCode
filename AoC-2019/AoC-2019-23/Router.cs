using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;

class Router
{
    private IReadOnlyDictionary<int, INode> _nodesById;

    public Router(IEnumerable<INode> nodes)
    {
        _nodesById = nodes.ToDictionary(node => node.Id);
    }

    public INode? GetDestination(Packet packet)
    {
        if (TryGetDestination(packet, out var node))
        {
            Console.ForegroundColor =
                packet.SourceId == NodeIds.Nat ? ConsoleColor.Green :
                packet.DestinationId == NodeIds.Nat ? ConsoleColor.Yellow :
                ConsoleColor.White;

            Console.WriteLine($"{packet}");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{packet} unrouteable");
            Console.ForegroundColor = ConsoleColor.White;
        }
        return node;
    }

    public bool TryGetDestination(Packet packet, [NotNullWhen(true)] out INode? destination)
    {
        return _nodesById.TryGetValue(packet.DestinationId, out destination);
    }
}
