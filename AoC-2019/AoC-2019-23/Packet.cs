record Packet(int SourceId, int DestinationId, Payload Payload)
{
    public override string ToString() => $"Packet[{SourceId}=>{DestinationId}] {Payload}";
}
