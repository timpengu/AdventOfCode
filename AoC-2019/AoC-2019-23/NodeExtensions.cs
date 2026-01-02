using System.Reactive.Linq;

static class NodeExtensions
{
    // TODO: Use Observable.FromEventPattern?
    public static IObservable<SentPacketEventArgs> ToSentPacketObservable(this INode node) =>
        Observable.FromEvent<EventHandler<SentPacketEventArgs>, SentPacketEventArgs>(
            action => (object? sender, SentPacketEventArgs e) => action(e),
            handler => node.SentPacket += handler,
            handler => node.SentPacket -= handler);
}