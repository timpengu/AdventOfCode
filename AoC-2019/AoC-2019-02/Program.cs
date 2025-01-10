
List<int> memory =
    File.ReadLines("input.txt")
    .Single()
    .Split(',', StringSplitOptions.RemoveEmptyEntries)
    .Select(int.Parse)
    .ToList();

// part 1
memory[1] = 12;
memory[2] = 02;

int ip = 0;

Dictionary<int, Action> ops = new()
{
    [1] = Add,
    [2] = Multiply,
    [99] = Halt,
};

bool halt = false;
while (!halt)
{
    Console.WriteLine($"[{ip}] {String.Join(',', memory)}");

    int opcode = Fetch();
    Action op = DecodeInstruction(opcode);
    op();
}

Console.WriteLine($"\nHalted with [0] = {Get(0)}");

int Fetch() => Get(ip++);
Action DecodeInstruction(int opcode) =>
    ops.TryGetValue(opcode, out Action op)
        ? op
        : throw new NotSupportedException($"Unknown opcode {opcode}");

void Add()
{
    ExecBinaryOp(
        Fetch(), Fetch(), Fetch(),
        (a, b) => a + b);
}

void Multiply()
{
    ExecBinaryOp(
        Fetch(), Fetch(), Fetch(),
        (a, b) => a * b);
}

void Halt()
{
    halt = true;
}

void ExecBinaryOp(int parameter1, int parameter2, int parameter3, Func<int,int,int> operation)
{
    int operand1 = Get(parameter1);
    int operand2 = Get(parameter2);
    int result = operation(operand1, operand2);
    Set(parameter3, result);
}

int Get(int address) => memory[address];
void Set(int address, int value) => memory[address] = value;
