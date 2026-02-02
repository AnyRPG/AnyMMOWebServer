namespace AnyMMOWebServer.Models
{
    public class AuctionItemListResponse
    {
        public List<AuctionItem> AuctionItems { get; set; }

        public AuctionItemListResponse()
        {
                AuctionItems = new List<AuctionItem>();
        }
    }
}
