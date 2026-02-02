namespace AnyMMOWebServer.Models
{
    public class CreateItemInstanceRequest
    {
        public long ItemInstanceId { get; set; }
        public string SaveData { get; set; }

        public CreateItemInstanceRequest()
        {
            SaveData = string.Empty;
        }
    }
}
