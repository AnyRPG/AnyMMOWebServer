namespace AnyMMOWebServer.Models
{
    public class AuthenticationResponse
    {
        public string Token { get; set; }

        public AuthenticationResponse()
        {
                Token = string.Empty;
        }
    }
}
