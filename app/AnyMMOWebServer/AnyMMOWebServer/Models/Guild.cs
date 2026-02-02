namespace AnyMMOWebServer.Models
{
    public class Guild
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SaveData { get; set; }

        public Guild() {
            Name = string.Empty;
            SaveData = string.Empty;
        }
    }
}
