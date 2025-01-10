class Computer
{
    private readonly Dictionary<int, Action> _ops;
    private readonly List<int> _memory;
    private readonly bool _verbose;
    private int _ip = 0;

    public Computer(IEnumerable<int> memory, bool verbose = false)
    {
        _memory = memory.ToList();
        _verbose = verbose;
        _ops = new()
        {
            [1] = Add,
            [2] = Multiply,
            [99] = Halt,
        };
    }

    public int MemorySize => _memory.Count;
    public IEnumerator<int> GetEnumerator() => _memory.GetEnumerator();
    public int this[int address]
    {
        get => _memory[address];
        set => _memory[address] = value;
    }

    public void Execute()
    {
        _ip = 0;
        while (_ip >= 0)
        {
            if (_verbose)
            {
                Console.WriteLine($"[{_ip}] {String.Join(',', _memory)}");
            }

            int opcode = Fetch();
            Action op = Decode(opcode);
            op();
        }
    }

    private void Add()
    {
        ExecBinaryOp((x, y) => x + y);
    }

    private void Multiply()
    {
        ExecBinaryOp((x, y) => x * y);
    }

    private void Halt()
    {
        _ip = -1;
    }

    private int Fetch() => this[_ip++];

    private Action Decode(int opcode) =>
        _ops.TryGetValue(opcode, out Action? op)
            ? op
            : throw new NotSupportedException($"Unknown opcode {opcode}");

    private void ExecBinaryOp(Func<int, int, int> operation) => ExecBinaryOp(operation, Fetch(), Fetch(), Fetch());
    private void ExecBinaryOp(Func<int, int, int> operation, int parameter1, int parameter2, int parameter3)
    {
        int operand1 = _memory[parameter1];
        int operand2 = _memory[parameter2];
        int result = operation(operand1, operand2);
        _memory[parameter3] = result;
    }
}
