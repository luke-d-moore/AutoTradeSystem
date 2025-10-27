using AutoTradeSystem;
using System.Text.Json.Serialization;

public class RemoveStrategyResponse : BaseResponse
{
    [JsonPropertyName("ID")]
    public string id;
    public RemoveStrategyResponse(bool success, string message, string ID) : base(success, message)
    {
        id = ID;
    }
}
