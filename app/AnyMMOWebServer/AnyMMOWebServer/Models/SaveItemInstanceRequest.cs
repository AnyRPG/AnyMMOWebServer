namespace AnyMMOWebServer.Models
{
    public class SaveItemInstanceRequest
    {
        public long ItemInstanceId { get; set; }
        public string SaveData { get; set; }

        public SaveItemInstanceRequest()
        {
            SaveData = string.Empty;
        }
    }
}
