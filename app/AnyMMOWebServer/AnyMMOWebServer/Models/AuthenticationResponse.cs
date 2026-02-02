namespace AnyMMOWebServer.Models
{
    public class AuthenticationResponse
    {
        public int AccountId { get; set; }
        public string Token { get; set; }

        public AuthenticationResponse()
        {
            Token = string.Empty;
        }
    }
}
