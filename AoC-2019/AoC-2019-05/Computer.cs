class Computer
{
    private readonly Dictionary<int, Action<Instruction>> _ops;
    private readonly List<int> _memory;
    private readonly Queue<int> _inputs;
    private readonly Queue<int> _outputs;
    private readonly bool _verbose;

    private int _ip;

    private record struct Instruction(int InstructionCode)
    {        
        public int OpCode => InstructionCode % 100;
        public int ParameterMode1 => InstructionCode / 100 % 10;
        public int ParameterMode2 => InstructionCode / 1000 % 10;
    }

    public Computer(IEnumerable<int> memory, IEnumerable<int>? inputs = null, bool verbose = false)
    {
        _ops = new()
        {
            [1] = Add,
            [2] = Multiply,
            [3] = Input,
            [4] = Output,
            [5] = JumpIfTrue,
            [6] = JumpIfFalse,
            [7] = LessThan,
            [8] = Equals,
            [99] = Halt,
        };

        _memory = new(memory);
        _inputs = new(inputs ?? []);
        _outputs = new();
        _verbose = verbose;
    }

    public int MemorySize => _memory.Count;
    public IEnumerator<int> GetEnumerator() => _memory.GetEnumerator();
    public int this[int address]
    {
        get => _memory[address];
        set => _memory[address] = value;
    }

    public IReadOnlyCollection<int> Inputs => _inputs;
    public IReadOnlyCollection<int> Outputs => _outputs;

    public void Execute()
    {
        _ip = 0;
        while (_ip >= 0)
        {
            if (_verbose)
            {
                Console.WriteLine($"[{_ip}] {String.Join(',', _memory)}");
            }

            Instruction instruction = new(Fetch());
            Action<Instruction> op = Decode(instruction.OpCode);
            op(instruction);
        }
    }

    private void Add(Instruction instruction) => BinaryOperator(instruction, (x, y) => x + y);
    private void Multiply(Instruction instruction) => BinaryOperator(instruction, (x, y) => x * y);
    private void LessThan(Instruction instruction) => BinaryOperator(instruction, (x, y) => x < y ? 1 : 0);
    private void Equals(Instruction instruction) => BinaryOperator(instruction, (x, y) => x == y ? 1 : 0);
    private void BinaryOperator(Instruction instruction, Func<int, int, int> binaryOperator)
    {
        int operand1 = LoadOperand(Fetch(), instruction.ParameterMode1);
        int operand2 = LoadOperand(Fetch(), instruction.ParameterMode2);
        int operand3 = Fetch();
        int result = binaryOperator(operand1, operand2);
        SaveResult(operand3, result);
    }

    private void Input(Instruction instruction)
    {
        int parameter = Fetch();
        int result = ReadInput();
        SaveResult(parameter, result);
    }

    private void Output(Instruction instruction)
    {
        int operand = LoadOperand(Fetch(), instruction.ParameterMode1);
        WriteOutput(operand);
    }

    private void JumpIfTrue(Instruction instruction) => JumpIf(instruction, x => x != 0);
    private void JumpIfFalse(Instruction instruction) => JumpIf(instruction, x => x == 0);
    private void JumpIf(Instruction instruction, Predicate<int> predicate)
    {
        int operand1 = LoadOperand(Fetch(), instruction.ParameterMode1);
        int operand2 = LoadOperand(Fetch(), instruction.ParameterMode2);
        if (predicate(operand1))
        {
            Jump(operand2);
        }
    }

    private void Halt(Instruction _)
    {
        Jump(-1);
    }

    private int Fetch() => _memory[_ip++];
    private void Jump(int ip) => _ip = ip;

    private Action<Instruction> Decode(int opcode) =>
        _ops.TryGetValue(opcode % 100, out Action<Instruction>? op)
            ? op
            : throw new NotSupportedException($"Unknown opcode: {opcode}");

    private int LoadOperand(int parameter, int parameterMode) =>
        parameterMode switch
        {
            0 => _memory[parameter], // position mode
            1 => parameter, // immediate mode
            _ => throw new NotSupportedException($"Unknown parameter mode: {parameterMode}")
        };

    private int SaveResult(int parameter, int result) =>
        _memory[parameter] = result; // always position mode

    private int ReadInput() =>
        _inputs.TryDequeue(out int input)
            ? input
            : throw new Exception("No input available");

    private void WriteOutput(int output) =>
        _outputs.Enqueue(output);
}
