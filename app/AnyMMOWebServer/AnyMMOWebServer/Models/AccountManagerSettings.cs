namespace AnyMMOWebServer.Models
{
    public class AnyMMOWebServerSettings
    {
        public AnyMMOWebServerSettings() {
            BearerKey = string.Empty;
        }
        public string BearerKey { get; set; }
    }
}
