using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace IntCode;

public class Computer<T>
    where T : struct, INumber<T>, ISignedNumber<T>
{
    private readonly Dictionary<byte, Action<Instruction>> _ops;
    private readonly Dictionary<T, T> _memory;
    private readonly IInputSource<T> _inputSource;
    private readonly Queue<T> _outputQueue;

    private enum ParameterMode : byte
    {
        Position = 0,
        Immediate = 1,
        Relative = 2
    }

    private record struct Instruction(int InstructionCode)
    {
        public Instruction(T instructionCode) : this(int.CreateChecked(instructionCode))
        {
            Debug.Assert(InstructionCode > 0);
        }

        public byte OpCode => (byte)(InstructionCode % 100);
        public ParameterMode ParameterMode1 => (ParameterMode)(InstructionCode / 100 % 10);
        public ParameterMode ParameterMode2 => (ParameterMode)(InstructionCode / 1000 % 10);
        public ParameterMode ParameterMode3 => (ParameterMode)(InstructionCode / 10000 % 10);
    }

    public Computer(IEnumerable<T> memory, Func<T> inputSource) : this(memory, new InputSource<T>(inputSource)) { }
    public Computer(IEnumerable<T> memory, params IEnumerable<T> inputs) : this(memory, inputs.ToInputSequence()) {}

    [OverloadResolutionPriority(1)]
    public Computer(IEnumerable<T> memory, IInputSource<T> inputSource)
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

        _memory = memory.Index().ToDictionary(m => T.CreateChecked(m.Index), m => m.Item);
        _inputSource = inputSource;
        _outputQueue = new();
    }

    public T Ip { get; private set; } = T.Zero;
    public T Rb { get; private set; } = T.Zero;
    public bool IsHalted => Ip < T.Zero;

    public T this[T address]
    {
        get => Load(address);
        set => Store(address, value);
    }

    public void ExecuteOne()
    {
        Debug.Assert(!IsHalted);
        Instruction instruction = new(Fetch());
        Action<Instruction> executor = Decode(instruction.OpCode);
        executor(instruction);
    }

    public IEnumerable<T> GetOutputs()
    {
        while (_outputQueue.TryDequeue(out T output))
        {
            yield return output;
        }
    }

    private void Add(Instruction instruction) => BinaryOperator(instruction, (x, y) => x + y);
    private void Multiply(Instruction instruction) => BinaryOperator(instruction, (x, y) => x * y);
    private void LessThan(Instruction instruction) => BinaryOperator(instruction, (x, y) => x < y ? T.One : T.Zero);
    private void Equals(Instruction instruction) => BinaryOperator(instruction, (x, y) => x == y ? T.One : T.Zero);
    private void BinaryOperator(Instruction instruction, Func<T, T, T> binaryOperator)
    {
        T operand1 = LoadOperand(Fetch(), instruction.ParameterMode1);
        T operand2 = LoadOperand(Fetch(), instruction.ParameterMode2);
        T result = binaryOperator(operand1, operand2);
        StoreResult(Fetch(), instruction.ParameterMode3, result);
    }

    private void Input(Instruction instruction)
    {
        T result = ReadInput();
        StoreResult(Fetch(), instruction.ParameterMode1, result);
    }

    private void Output(Instruction instruction)
    {
        T operand = LoadOperand(Fetch(), instruction.ParameterMode1);
        WriteOutput(operand);
    }

    private void JumpIfTrue(Instruction instruction) => JumpIf(instruction, x => x != T.Zero);
    private void JumpIfFalse(Instruction instruction) => JumpIf(instruction, x => x == T.Zero);
    private void JumpIf(Instruction instruction, Predicate<T> predicate)
    {
        T operand1 = LoadOperand(Fetch(), instruction.ParameterMode1);
        T operand2 = LoadOperand(Fetch(), instruction.ParameterMode2);
        if (predicate(operand1))
        {
            Jump(operand2);
        }
    }

    private void AdjustRelativeBase(Instruction instruction)
    {
        T operand = LoadOperand(Fetch(), instruction.ParameterMode1);
        Rb += operand;
    }

    private void Halt(Instruction _)
    {
        Jump(T.NegativeOne);
    }

    private T Fetch() => Load(Ip++);
    private void Jump(T ip) => Ip = ip;

    private Action<Instruction> Decode(byte opcode) =>
        _ops.TryGetValue(opcode, out Action<Instruction>? op) ? op
            : throw new NotSupportedException($"Unknown opcode: {opcode}");

    private T LoadOperand(T parameter, ParameterMode mode) => mode switch
    {
        ParameterMode.Immediate => parameter,
        ParameterMode.Position => Load(parameter),
        ParameterMode.Relative => Load(Rb + parameter),
        _ => throw new NotSupportedException($"Unknown parameter mode: {mode}")
    };

    private void StoreResult(T parameter, ParameterMode mode, T value)
    {
        T address = mode switch
        {
            ParameterMode.Immediate => throw new InvalidOperationException($"Invalid parameter mode for store: {mode}"),
            ParameterMode.Position => parameter,
            ParameterMode.Relative => Rb + parameter,
            _ => throw new NotSupportedException($"Unknown parameter mode: {mode}")
        };
        Store(address, value);
    }

    private T Load(T address)
    {
        Debug.Assert(address >= T.Zero);
        return _memory.TryGetValue(address, out T value) ? value : T.Zero;
    }

    private void Store(T address, T value)
    {
        Debug.Assert(address >= T.Zero);
        _memory[address] = value;
    }

    private T ReadInput() => _inputSource.ReadInput();
    private void WriteOutput(T output) => _outputQueue.Enqueue(output);
}
