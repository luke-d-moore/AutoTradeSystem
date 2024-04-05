using AutoTradeSystem;

public class RemoveStrategyResponse :Response
{
    readonly string _id;
    public RemoveStrategyResponse(bool success, string message, string ID) : base(success, message)
    {
        _id = ID;
    }
}
