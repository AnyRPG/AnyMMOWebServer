namespace AnyMMOWebServer.Models
{
    public class SaveAuctionItemRequest
    {
        public int Id { get; set; }
        public string SaveData { get; set; }

        public SaveAuctionItemRequest()
        {
            SaveData = string.Empty;
        }
    }
}
