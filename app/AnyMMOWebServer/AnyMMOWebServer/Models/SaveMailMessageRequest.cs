namespace AnyMMOWebServer.Models
{
    public class SaveMailMessageRequest
    {
        public int Id { get; set; }
        public string SaveData { get; set; }

        public SaveMailMessageRequest()
        {
            SaveData = string.Empty;
        }
    }
}
