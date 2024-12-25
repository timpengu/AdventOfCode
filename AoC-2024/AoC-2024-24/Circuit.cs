using System.Diagnostics;

class Circuit
{
    private readonly bool _verbose;
    private readonly List<Gate> _gates;
    private readonly ILookup<string, Gate> _signalGates;

    private Dictionary<string, bool> _signals = new();

    public Circuit(
        IEnumerable<Gate> gates,
        IEnumerable<KeyValuePair<string, bool>>? signals = null,
        bool verbose = false)
    {
        _verbose = verbose;
        _gates = new(gates);
        _signalGates = gates
            .SelectMany(g => g.Inputs, (gate, input) => (Gate: gate, Input: input))
            .ToLookup(g => g.Input, g => g.Gate);

        ResetSignals(signals);
    }

    public void ResetSignals(IEnumerable<KeyValuePair<string,bool>>? signals = null)
    {
        _signals = new(signals ?? Enumerable.Empty<KeyValuePair<string, bool>>());
    }

    public ulong GetRegister(char register)
    {
        return Bits.GetBitIndexes().Aggregate(0ul, (result, bit) => result |= GetRegisterBitValue(bit));

        ulong GetRegisterBitValue(int bit)
        {
            string key = GetSignal(register, bit);
            bool bitValue = _signals.TryGetValue(key, out bool v) && v;
            return bitValue ? bit.ToBitValue() : 0ul;
        }
    }

    public void SetRegister(char register, ulong value)
    {
        Debug.Assert(value < Bits.OverflowValue);
        foreach (var bit in Bits.GetBitIndexes())
        {
            string key = GetSignal(register, bit);
            _signals[key] = (value & bit.ToBitValue()) != 0;
        }
    }

    public IEnumerable<string> GetDependencies(string signal, int depth)
    {
        Dictionary<string, Gate> outputGates = _gates.ToDictionary(g => g.Output);
        HashSet<string> deps = new();
        GetDependencies(signal, depth);
        return deps;

        void GetDependencies(string signal, int depth)
        {
            if (depth > 0 && outputGates.ContainsKey(signal))
            {
                deps.Add(signal);

                Gate gate = outputGates[signal];
                GetDependencies(gate.Input1, depth - 1);
                GetDependencies(gate.Input2, depth - 1);
            }
        }
    }

    public IEnumerable<string> GetDependents(string signal, int depth)
    {
        HashSet<string> deps = new();
        GetDependents(signal, depth);
        return deps;

        void GetDependents(string signal, int depth)
        {
            if (depth == 0)
                return;

            IEnumerable<Gate> gates = _signalGates[signal];
            foreach (Gate gate in gates)
            {
                deps.Add(gate.Output);
                GetDependents(gate.Output, depth - 1);
            }
        }
    }

    public void PropagateSignals()
    {
        Debug.Assert(!_gates.Select(g => g.Output).Any(_signals.ContainsKey));

        Queue<string> signalPropagationQueue = new(_signals.Keys);
        while (signalPropagationQueue.TryDequeue(out string? signal))
        {
            foreach (Gate gate in _signalGates[signal])
            {
                if (TryPropagate(gate, out bool value))
                {
                    _signals[gate.Output] = value;
                    signalPropagationQueue.Enqueue(gate.Output);
                }
            }
        }
    }

    private bool TryPropagate(Gate gate, out bool output)
    {
        if (_signals.ContainsKey(gate.Output) || // output already propagted
            !gate.Inputs.All(_signals.ContainsKey)) // inputs not available
        {
            output = default;
            return false;
        }

        bool input1 = _signals[gate.Input1];
        bool input2 = _signals[gate.Input2];

        output = gate.Operator switch
        {
            BooleanOperator.And => input1 & input2,
            BooleanOperator.Or => input1 | input2,
            BooleanOperator.Xor => input1 ^ input2,
            _ => throw new NotSupportedException($"Unknown {nameof(BooleanOperator)}: {gate.Operator}")
        };

        if (_verbose)
        {
            Console.WriteLine($"{gate.Input1}:{input1.ToInt()} {gate.Operator.ToString().ToUpper()} {gate.Input2}:{input2.ToInt()} => {gate.Output}:{output.ToInt()}");
        }

        return true;
    }

    public static int ToInt(bool value) => value ? 1 : 0;
    public static string GetSignal(char register, int bit) => $"{register}{bit:00}";
}
