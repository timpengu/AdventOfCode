class SentPacketEventArgs(Packet packet) : EventArgs
{
    public Packet Packet { get; } = packet;
}
