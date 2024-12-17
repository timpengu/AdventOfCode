record struct Opval(int? Out, int? Jmp);
record struct Op(int Opcode, string Name, Func<int,Opval> Execute)
{
    public static Op Create(int opcode, string name, Action<int> execute) =>
        new Op(opcode, name, operand =>
        {
            execute(operand);
            return new(null, null);
        });

    public static Op CreateJmp(int opcode, string name, Func<int, int?> execute) =>
        new Op(opcode, name, operand =>
        {
            int? ipJmp = execute(operand);
            return new(null, ipJmp);
        });

    public static Op CreateOut(int opcode, string name, Func<int, int> execute) =>
        new Op(opcode, name, operand =>
        {
            int output = execute(operand);
            return new(output, null);
        });
}
