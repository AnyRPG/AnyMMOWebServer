using System.Security.Permissions;

namespace AnyMMOWebServer.Models
{
    public class AuthenticationRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public AuthenticationRequest()
        {
            UserName = string.Empty;
            Password = string.Empty;
        }

        public AuthenticationRequest(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }

        public AuthenticationRequest(IFormCollection collection)
        {
            // put form into variables
            if (collection != null) {
                if (collection.ContainsKey("username")) {
                    UserName = collection["username"];
                }
                Password = collection["password"];
            }
        }

    }
}
