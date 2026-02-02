namespace AnyMMOWebServer.Models
{
    public class GuildListResponse
    {
        public List<Guild> Guilds { get; set; }

        public GuildListResponse()
        {
                Guilds = new List<Guild>();
        }
    }
}
