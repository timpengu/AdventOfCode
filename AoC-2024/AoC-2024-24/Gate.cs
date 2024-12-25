record struct Gate(BooleanOperator Operator, string Input1, string Input2, string Output)
{
    public IEnumerable<string> Inputs => [Input1, Input2];
}
