namespace AnyMMOWebServer.Models
{
    public class ItemInstanceListResponse
    {
        public List<ItemInstance> ItemInstances { get; set; }

        public ItemInstanceListResponse()
        {
                ItemInstances = new List<ItemInstance>();
        }
    }
}
