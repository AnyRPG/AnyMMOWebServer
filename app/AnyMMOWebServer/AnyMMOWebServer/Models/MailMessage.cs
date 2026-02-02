namespace AnyMMOWebServer.Models
{
    public class MailMessage
    {
        public int Id { get; set; }
        public int PlayerCharacterId { get; set; }
        public string SaveData { get; set; }

        public MailMessage() {
            SaveData = string.Empty;
        }
    }
}
