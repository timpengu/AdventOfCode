using IntCode;

class Nic
{
    public class SentPacketEventArgs(Packet packet) : EventArgs
    {
        public Packet Packet { get; } = packet;
    }

    public int Id { get; }
    public event EventHandler<SentPacketEventArgs>? SentPacket;

    private readonly Computer<long> _computer;
    private readonly Queue<long> _inputs = new();
    private readonly Queue<long> _outputs = new(3);

    public Nic(IEnumerable<long> program, int id)
    {
        Id = id;

        _computer = new Computer<long>(program, ReadInput);
        _inputs.Enqueue(id);
    }

    public async Task ExecuteAsync()
    {
        Console.WriteLine($"{this} started");

        while (!_computer.IsHalted)
        {
            // Console.WriteLine($"{this} @{_computer.Ip}: {_computer[_computer.Ip]}");
            _computer.ExecuteOne();

            foreach (long output in _computer.GetOutputs())
            {
                WriteOutput(output);
            }

            await Task.Yield(); // TODO: yield on every Nth loop?
        }

        Console.WriteLine($"{this} stopped");
    }

    public void ReceivePacket(Packet packet)
    {
        if (packet.DestinationId != Id)
            throw new InvalidOperationException($"Packet for NIC[{packet.DestinationId}] received by NIC[{Id}]");

        lock (_inputs)
        {
            _inputs.Enqueue(packet.Payload.X);
            _inputs.Enqueue(packet.Payload.Y);
        }
    }

    private long ReadInput()
    {
        lock (_inputs)
        {
            long input = _inputs.TryDequeue(out var value) ? value : -1;
            // Console.WriteLine($"{this} in {input}");
            return input;
        }
    }

    private void WriteOutput(long output)
    {
        // Console.WriteLine($"{this} out {output}");
        _outputs.Enqueue(output);

        while (_outputs.Count >= 3)
        {
            int destId = (int) _outputs.Dequeue();
            long x = _outputs.Dequeue();
            long y = _outputs.Dequeue();
            
            var packet = new Packet(Id, destId, new Payload(x, y));
            
            OnSendPacket(packet);
        }
    }

    private void OnSendPacket(Packet packet) => SentPacket?.Invoke(this, new SentPacketEventArgs(packet));

    public override string ToString() => $"NIC[{Id}]";
}
