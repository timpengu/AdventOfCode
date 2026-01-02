class Nat : INode
{
    public int Id { get; }
    public int DestinationId { get; }

    public Payload? NextPayload { get; private set; } = null;
    public Payload? PrevPayload { get; private set; } = null;

    private Func<bool> IsNetworkIdle { get; }

    public event EventHandler<SentPacketEventArgs>? SentPacket;

    public Nat(int id, int destinationId, Func<bool> isNetworkIdle)
    {
        Id = id;
        DestinationId = destinationId;
        IsNetworkIdle = isNetworkIdle;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"{this} started");

        while (!cancellationToken.IsCancellationRequested)
        {
            if (NextPayload != null && IsNetworkIdle())
            {
                Packet packet = new(Id, DestinationId, NextPayload);
                OnSendPacket(packet);

                if (PrevPayload == null)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"{this} sent first packet");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (PrevPayload == NextPayload)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"{this} sent repeated packet");
                    Console.ForegroundColor = ConsoleColor.White;

                    break; // can stop NAT now
                }

                PrevPayload = NextPayload;
            }

            // TODO: Pass in a semaphore to signal when network goes idle instead of polling?
            await Task.Delay(500, cancellationToken);
        }

        Console.WriteLine($"{this} stopped");
    }

    public void ReceivePacket(Packet packet)
    {
        if (packet.DestinationId != Id)
            throw new InvalidOperationException($"Packet for NIC[{packet.DestinationId}] received by {this}");

        NextPayload = packet.Payload;
    }

    private void OnSendPacket(Packet packet) => SentPacket?.Invoke(this, new SentPacketEventArgs(packet));

    public override string ToString() => $"NAT[{Id}]";
}
