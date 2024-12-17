using System.Diagnostics;

class Computer
{
    private enum Register { A = 0, B = 1, C = 2 }

    private readonly long[] _registers = new long[3];
    private readonly Op[] _ops;
    
    public bool IsVerbose = false;

    public Computer()
    {
        _ops = new Op[]
        {
            Op.Create(0, "adv", Adv),
            Op.Create(1, "bxl", Bxl),
            Op.Create(2, "bst", Bst),
            Op.CreateJmp(3, "jnz", Jnz),
            Op.Create(4, "bxc", Bxc),
            Op.CreateOut(5, "out", Out),
            Op.Create(6, "bdv", Bdv),
            Op.Create(7, "cdv", Cdv),
        };

        Debug.Assert(!_ops.Where((op,i) => op.Opcode != i).Any());
    }

    public Computer(IEnumerable<(char Reg, long Value)> registers) : this()
    {
        InitRegisters(registers);
    }

    public void InitRegisters(IEnumerable<(char Reg, long Value)> registers)
    {
        foreach ((char reg, long value) in registers)
        {
            SetValue(GetRegisterIndex(reg), value);
        }
    }

    public IEnumerable<int> Execute(IReadOnlyList<int> program)
    {
        if (IsVerbose)
        {
            Console.WriteLine($"Program: {string.Join(',', program)}");
            Console.WriteLine($"Registers: {GetRegistersString()}");
        }

        int ip = 0;
        while (ip < program.Count)
        {
            int opcode = program[ip];
            int operand = program[ip + 1];

            Op op = Decode(opcode);
            Opval opval = op.Execute(operand);

            if (IsVerbose)
            {
                string operandName = GetOperandString(op, operand);
                string registers = GetRegistersString();
                string result = opval.Out.HasValue ? $"=> {opval.Out}" : opval.Jmp.HasValue ? "J" : "";
                Console.WriteLine($"[{ip}]\t{op.Name} {operandName} \t{registers} {result}");
            }

            if (opval.Out.HasValue)
            {
                yield return opval.Out.Value;
            }

            ip = opval.Jmp ?? ip + 2;
        }
    }

    private void Adv(int operand) => Xdv(operand, Register.A);
    private void Bdv(int operand) => Xdv(operand, Register.B);
    private void Cdv(int operand) => Xdv(operand, Register.C);
    private void Xdv(int operand, Register dest)
    {
        long num = GetValue(Register.A);
        long div =  GetCombo(operand);
        long result = div > 64 ? 0 : num >> (int)div;
        SetValue(dest, result);
    }

    private void Bxc(int _) => Bx(GetValue(Register.C));
    private void Bxl(int operand) => Bx(operand);
    private void Bx(long operand)
    {
        long result = GetValue(Register.B) ^ operand;
        SetValue(Register.B, result);
    }

    private void Bst(int operand)
    {
        long result = GetCombo(operand) & 0b111;
        SetValue(Register.B, result);
    }

    private int? Jnz(int operand)
    {
        long test = GetValue(Register.A);
        return test != 0 ? operand : null;
    }

    private int Out(int operand)
    {
        long result = GetCombo(operand) & 0b111;
        return (int) result;
    }

    private Op Decode(int opcode) =>
        opcode >= 0 && opcode < _ops.Length
        ? _ops[opcode]
        : throw new Exception($"Unknown opcode: {opcode}");

    private long GetCombo(int operand) => operand switch
    {
        >= 0 and <= 3 => operand,
        4 => GetValue(Register.A),
        5 => GetValue(Register.B),
        6 => GetValue(Register.C),
        _ => throw new Exception($"Unknown combo: {operand}")
    };

    private long GetValue(Register reg) => _registers[(int)reg];
    private void SetValue(Register reg, long value) => _registers[(int)reg] = value;

    private Register GetRegisterIndex(char c) => c switch
    {
        'A' => Register.A,
        'B' => Register.B,
        'C' => Register.C,
        _ => throw new Exception($"Unknown register: {c}")
    };

    private string GetRegistersString()
    {
        char[] regs = ['A', 'B', 'C'];
        return String.Join(",",
            from reg in regs
            let value = GetValue(GetRegisterIndex(reg))
            select $"{reg}={value.ToOctalString()}"
        );
    }

    private static string GetOperandString(Op op, int operand)
    {
        bool isCombo = op.Name is "adv" or "bdv" or "cdv" or "bst" or "out";
        return (isCombo, operand) switch
        {
            (true, 4) => "A",
            (true, 5) => "B",
            (true, 6) => "C",
            _ => operand.ToString(),
        };
    }
}
