namespace AutoTradeSystem
{
    public abstract class Response
    {
        readonly bool Success;
        readonly string Message;
        readonly DateTime TimeStamp;
        protected Response(bool success, string message) 
        { 
            Success = success;
            Message = message;
            TimeStamp = DateTime.Now;
        }
    }
}
