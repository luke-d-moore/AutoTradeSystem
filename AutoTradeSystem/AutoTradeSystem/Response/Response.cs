namespace AutoTradeSystem
{
    public class Response
    {
        public bool Success;
        public string Message;
        public DateTime TimeStamp;
        protected Response(bool success, string message) 
        { 
            Success = success;
            Message = message;
            TimeStamp = DateTime.Now;
        }
    }
}
