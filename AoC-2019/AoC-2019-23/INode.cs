interface INode
{
    int Id { get; }

    Task ExecuteAsync(CancellationToken cancellationToken);

    void ReceivePacket(Packet packet);
    event EventHandler<SentPacketEventArgs>? SentPacket;
}
