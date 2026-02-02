namespace AnyMMOWebServer.Models
{
    public class FriendListResponse
    {
        public List<FriendList> FriendLists { get; set; }

        public FriendListResponse()
        {
            FriendLists = new List<FriendList>();
        }
    }
}
