using AutoTradeSystem;
using System.Text.Json.Serialization;

public class RemoveStrategyResponse :Response
{
    [JsonPropertyName("ID")]
    public string id;
    public RemoveStrategyResponse(bool success, string message, string ID) : base(success, message)
    {
        id = ID;
    }
}
