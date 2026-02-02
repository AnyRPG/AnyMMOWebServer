namespace AnyMMOWebServer.Models
{
    public class AuctionItem
    {
        public int Id { get; set; }
        public string SaveData { get; set; }

        public AuctionItem() {
            SaveData = string.Empty;
        }
    }
}
