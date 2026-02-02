namespace AnyMMOWebServer.Models
{
    public class FriendList
    {
        public int Id { get; set; }
        public int PlayerCharacterId { get; set; }
        public string SaveData { get; set; }

        public FriendList() {
            SaveData = string.Empty;
        }
    }
}
