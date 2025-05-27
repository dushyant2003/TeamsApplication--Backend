namespace TeamsApplicationServer.Model
{
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class SendMessageModel
    {
        public string ReceiverId { get; set; }

        public string ReceiverName { get; set; }

        public string Message { get; set; }

        public string SenderId { get; set; }

        public string SenderName { get; set; }

        public long TimeStamp { get; set; }

        public string Type { get; set; }        // Group or Individual
    }
}
