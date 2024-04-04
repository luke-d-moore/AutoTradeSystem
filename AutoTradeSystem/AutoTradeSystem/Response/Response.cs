namespace AutoTradeSystem
{
    public class Response
    {
        bool Success;
        string Message;
        DateTime TimeStamp;
        public Response() { }
        public Response(bool success, string message) 
        { 
            Success = success;
            Message = message;
            TimeStamp = DateTime.Now;
        }
    }
}
