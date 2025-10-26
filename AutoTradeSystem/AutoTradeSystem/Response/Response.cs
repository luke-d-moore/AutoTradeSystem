namespace AutoTradeSystem
{
    public class Response
    {
        public bool Success { get; protected set; }
        public string Message { get; protected set; }
        public DateTime TimeStamp { get; protected set; }
        protected Response(bool success, string message)
        {
            Success = success;
            Message = message;
            TimeStamp = DateTime.Now;
        }
    }
}
