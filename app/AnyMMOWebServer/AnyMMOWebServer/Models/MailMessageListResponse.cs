namespace AnyMMOWebServer.Models
{
    public class MailMessageListResponse
    {
        public List<MailMessage> MailMessages { get; set; }

        public MailMessageListResponse()
        {
                MailMessages = new List<MailMessage>();
        }
    }
}
