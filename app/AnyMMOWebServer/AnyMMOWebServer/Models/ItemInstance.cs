namespace AnyMMOWebServer.Models
{
    public class ItemInstance
    {
        public int Id { get; set; }
        public long ItemInstanceId { get; set; }
        public string SaveData { get; set; }

        public ItemInstance() {
            SaveData = string.Empty;
        }
    }
}
