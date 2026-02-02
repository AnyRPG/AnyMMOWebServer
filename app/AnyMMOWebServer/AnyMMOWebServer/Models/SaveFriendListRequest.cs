namespace AnyMMOWebServer.Models
{
    public class SaveFriendListRequest
    {
        public int PlayerCharacterId { get; set; }
        public string SaveData { get; set; }

        public SaveFriendListRequest()
        {
            SaveData = string.Empty;
        }
    }
}
