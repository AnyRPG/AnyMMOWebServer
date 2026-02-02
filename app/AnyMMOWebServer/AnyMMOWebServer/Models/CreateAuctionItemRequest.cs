namespace AnyMMOWebServer.Models
{
    public class CreateAuctionItemRequest
    {
        public string SaveData { get; set; }

        public CreateAuctionItemRequest()
        {
            SaveData = string.Empty;
        }
    }
}
