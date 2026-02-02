namespace AnyMMOWebServer.Models
{
    public class AnyMMOWebServerSettings
    {
        public string BearerKey { get; set; }
        public string SharedSecret { get; set; }
        
        public AnyMMOWebServerSettings() {
            BearerKey = string.Empty;
            SharedSecret = string.Empty;
        }
    }
}
