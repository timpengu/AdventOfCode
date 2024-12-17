using System.Text.RegularExpressions;

Dictionary<char, int> registers = new();
List<int> program = new();
List<int> output = new();

Op[] ops = {
    new Op(0, "adv", Adv),
    new Op(1, "bxl", Bxl),
    new Op(2, "bst", Bst),
    new Op(3, "jnz", Jnz),
    new Op(4, "bxc", Bxc),
    new Op(5, "out", Out),
    new Op(6, "bdv", Bdv),
    new Op(7, "cdv", Cdv),
};

foreach (string line in File.ReadLines("inputSample.txt").Where(l => l.Length > 0))
{
    if (TryParseRegister(line, out char reg, out int value))
    {
        registers[reg] = value;
    }
    else if (TryParseProgram(line, out program))
    {
    }
    else throw new InvalidDataException($"Unknown input: {line}");
}

IDictionary<int, Op> opTable = ops.ToDictionary(op => op.Opcode);

// part 1
Console.WriteLine($"Program: {String.Join(',', program)}\n");
Execute(program);
Console.WriteLine($"\nOutput: {String.Join(',', output)}\n");

void Execute(IList<int> program)
{
    int ip = 0;
    while (ip < program.Count)
    {
        int opcode = program[ip];
        int operand = program[ip + 1];

        Op op = Decode(opcode);
        Console.WriteLine($"{ip}: {op.Name} {operand}");

        int? ipJmp = op.Execute(operand);
        ip = ipJmp ?? ip + 2;
    }
}

void Adv(int operand) => Xdv(operand, 'A');
void Bdv(int operand) => Xdv(operand, 'B');
void Cdv(int operand) => Xdv(operand, 'C');
void Xdv(int operand, char dest)
{
    int num = GetRegister('A');
    int div = 1 << GetCombo(operand);
    int result = num / div;
    SetRegister(dest, result);
}

void Bxc(int _) => Bxl(GetRegister('C'));
void Bxl(int operand)
{
    int result = GetRegister('B') ^ operand;
    SetRegister('B', result);
}

void Bst(int operand)
{
    int result = GetCombo(operand) % 8;
    SetRegister('B', result);
}

int? Jnz(int operand)
{
    int test = GetRegister('A');
    return test != 0 ? operand : null;
}

void Out(int operand)
{
    int result = GetCombo(operand) % 8;
    output.Add(result);
}

Op Decode(int opcode) =>
    opTable.TryGetValue(opcode, out Op op)
    ? op
    : throw new Exception($"Unknown opcode: {opcode}");

int GetCombo(int operand) => operand switch
{
    >= 0 and <= 3 => operand,
    4 => GetRegister('A'),
    5 => GetRegister('B'),
    6 => GetRegister('C'),
    _ => throw new Exception($"Unknown combo: {operand}")
};

int GetRegister(char reg) =>
    registers.TryGetValue(reg, out int value)
        ? value
        : throw new Exception($"Unknown register: {reg}");

void SetRegister(char reg, int value)
{
    if (!registers.ContainsKey(reg))
        throw new Exception($"Unknown register: {reg}");

    registers[reg] = value;
}

static bool TryParseRegister(string line, out char reg, out int value)
{
    Match match = Regex.Match(line, @"^Register ([A-Z]): ([0-9]+)$");
    if (match.Success &&
        match.Groups[1].Value.Length == 1 &&
        int.TryParse(match.Groups[2].Value, out value))
    {
        reg = match.Groups[1].Value.Single();
        return true;
    }

    (reg, value) = (default, default);
    return false;
}

static bool TryParseProgram(string line, out List<int> program)
{
    Match match = Regex.Match(line, @"^Program: ([0-9, ]+)$");
    if (match.Success)
    {
        List<int?> values = match.Groups[1].Value.Split(',').Select(s => int.TryParse(s, out int value) ? (int?)value : null).ToList();
        if (values.All(v => v.HasValue))
        {
            program = values.Select(v => v.Value).ToList();
            return true;
        }
    }

    program = [];
    return false;
}

record struct Op(int Opcode, string Name, Func<int,int?> Execute)
{
    public Op(int opcode, string name, Action<int> executeNonJump) :
        this(opcode, name, operand => ExecuteNonJump(executeNonJump, operand))
    {
    }

    private static int? ExecuteNonJump(Action<int> nonJumpExecutor, int operand)
    {
        nonJumpExecutor(operand);
        return null;
    }
}