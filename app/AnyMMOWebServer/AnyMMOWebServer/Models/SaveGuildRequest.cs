namespace AnyMMOWebServer.Models
{
    public class SaveGuildRequest
    {
        public int Id { get; set; }
        public string SaveData { get; set; }

        public SaveGuildRequest()
        {
            SaveData = string.Empty;
        }
    }
}
