using System.Security.Permissions;

namespace AnyMMOWebServer.Models
{
    public class ServerAuthenticationRequest
    {
        public string SharedSecret { get; set; }

        public ServerAuthenticationRequest()
        {
            SharedSecret = string.Empty;
        }

        public ServerAuthenticationRequest(string sharedSecret)
        {
            SharedSecret = sharedSecret;
        }

    }
}
