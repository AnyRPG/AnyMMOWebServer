namespace AnyMMOWebServer.Models
{
    public class CreateFriendListRequest
    {
        public int PlayerCharacterId { get; set; }
        public string SaveData { get; set; }

        public CreateFriendListRequest()
        {
            SaveData = string.Empty;
        }
    }
}
