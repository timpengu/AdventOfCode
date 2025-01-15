using System.Diagnostics;

class Computer
{
    private readonly Dictionary<byte, Action<Instruction>> _ops;
    private readonly Dictionary<long, long> _memory;
    private readonly IInputSource _inputSource;
    private readonly Queue<long> _outputQueue;

    private enum ParameterMode : byte
    {
        Position = 0,
        Immediate = 1,
        Relative = 2
    }

    private record struct Instruction(long InstructionCode)
    {
        public byte OpCode => (byte)(InstructionCode % 100);
        public ParameterMode ParameterMode1 => (ParameterMode)(InstructionCode / 100 % 10);
        public ParameterMode ParameterMode2 => (ParameterMode)(InstructionCode / 1000 % 10);
        public ParameterMode ParameterMode3 => (ParameterMode)(InstructionCode / 10000 % 10);
    }

    public Computer(IEnumerable<long> memory, params long[] inputs) : this(memory, new InputSequence(inputs))
    {
    }

    public Computer(IEnumerable<long> memory, IInputSource inputSource)
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
            [9] = AdjustRelativeBase,
            [99] = Halt,
        };

        _memory = memory.Index().ToDictionary(m => (long)m.Index, m => m.Item);
        _inputSource = inputSource;
        _outputQueue = new();
    }

    public long Ip { get; private set; } = 0;
    public long Rb { get; private set; } = 0;
    public bool IsHalted => Ip < 0;

    public long this[long address]
    {
        get => Load(address);
        set => Store(address, value);
    }

    public IEnumerable<long> ExecuteOutputs()
    {
        while (ExecuteOne())
        {
            foreach(var output in GetOutputs())
            {
                yield return output;
            }
        }
    }

    public bool ExecuteOne()
    {
        if (IsHalted)
        {
            return false;
        }

        Instruction instruction = new(Fetch());
        Action<Instruction> op = Decode(instruction.OpCode);
        op(instruction);

        return true;
    }

    public IEnumerable<long> GetOutputs()
    {
        while (_outputQueue.TryDequeue(out long output))
        {
            yield return output;
        }
    }

    private void Add(Instruction instruction) => BinaryOperator(instruction, (x, y) => x + y);
    private void Multiply(Instruction instruction) => BinaryOperator(instruction, (x, y) => x * y);
    private void LessThan(Instruction instruction) => BinaryOperator(instruction, (x, y) => x < y ? 1 : 0);
    private void Equals(Instruction instruction) => BinaryOperator(instruction, (x, y) => x == y ? 1 : 0);
    private void BinaryOperator(Instruction instruction, Func<long, long, long> binaryOperator)
    {
        long operand1 = LoadOperand(Fetch(), instruction.ParameterMode1);
        long operand2 = LoadOperand(Fetch(), instruction.ParameterMode2);
        long result = binaryOperator(operand1, operand2);
        StoreResult(Fetch(), instruction.ParameterMode3, result);
    }

    private void Input(Instruction instruction)
    {
        long result = ReadInput();
        StoreResult(Fetch(), instruction.ParameterMode1, result);
    }

    private void Output(Instruction instruction)
    {
        long operand = LoadOperand(Fetch(), instruction.ParameterMode1);
        WriteOutput(operand);
    }

    private void JumpIfTrue(Instruction instruction) => JumpIf(instruction, x => x != 0);
    private void JumpIfFalse(Instruction instruction) => JumpIf(instruction, x => x == 0);
    private void JumpIf(Instruction instruction, Predicate<long> predicate)
    {
        long operand1 = LoadOperand(Fetch(), instruction.ParameterMode1);
        long operand2 = LoadOperand(Fetch(), instruction.ParameterMode2);
        if (predicate(operand1))
        {
            Jump(operand2);
        }
    }

    private void AdjustRelativeBase(Instruction instruction)
    {
        long operand = LoadOperand(Fetch(), instruction.ParameterMode1);
        Rb += operand;
    }

    private void Halt(Instruction _)
    {
        Jump(-1);
    }

    private long Fetch() => Load(Ip++);
    private void Jump(long ip) => Ip = ip;

    private Action<Instruction> Decode(byte opcode) =>
        _ops.TryGetValue(opcode, out Action<Instruction>? op) ? op
            : throw new NotSupportedException($"Unknown opcode: {opcode}");

    private long LoadOperand(long parameter, ParameterMode mode) => mode switch
    {
        ParameterMode.Immediate => parameter,
        ParameterMode.Position => Load(parameter),
        ParameterMode.Relative => Load(Rb + parameter),
        _ => throw new NotSupportedException($"Unknown parameter mode: {mode}")
    };

    private void StoreResult(long parameter, ParameterMode mode, long value)
    {
        long address = mode switch
        {
            ParameterMode.Immediate => throw new InvalidOperationException($"Invalid parameter mode for store: {mode}"),
            ParameterMode.Position => parameter,
            ParameterMode.Relative => Rb + parameter,
            _ => throw new NotSupportedException($"Unknown parameter mode: {mode}")
        };
        Store(address, value);
    }

    private long Load(long address)
    {
        Debug.Assert(address >= 0);
        return _memory.TryGetValue(address, out long value) ? value : 0L;
    }

    private void Store(long address, long value)
    {
        Debug.Assert(address >= 0);
        _memory[address] = value;
    }

    private long ReadInput() => _inputSource.ReadInput();
    private void WriteOutput(long output) => _outputQueue.Enqueue(output);
}
