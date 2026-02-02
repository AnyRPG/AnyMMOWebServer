namespace AnyMMOWebServer.Models
{
    public class CreateMailMessageRequest
    {
        public int PlayerCharacterId { get; set; }
        public string SaveData { get; set; }

        public CreateMailMessageRequest()
        {
            SaveData = string.Empty;
        }
    }
}
