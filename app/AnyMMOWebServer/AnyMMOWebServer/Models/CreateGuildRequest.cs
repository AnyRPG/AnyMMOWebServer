namespace AnyMMOWebServer.Models
{
    public class CreateGuildRequest
    {
        public string SharedSecret { get; set; }
        public string SaveData { get; set; }

        public CreateGuildRequest()
        {
            SharedSecret = string.Empty;
            SaveData = string.Empty;
        }
    }
}
