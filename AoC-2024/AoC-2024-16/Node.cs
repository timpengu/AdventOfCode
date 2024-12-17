record Node(State State)
{
    public int Cost { get; set; } = int.MaxValue;
    public List<Node> Prev { get; set; } = new();
}
