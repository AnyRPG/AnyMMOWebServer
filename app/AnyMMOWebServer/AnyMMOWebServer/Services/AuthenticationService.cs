using AnyMMOWebServer.Database;
using AnyMMOWebServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AnyMMOWebServer.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AnyMMOWebServerSettings anyMMOWebServerSettings;
        private readonly GameDbContext gameDbContext;
        private readonly ILogger<AuthenticationService> logger;

        public AuthenticationService(AnyMMOWebServerSettings anyMMOWebServerSettings, GameDbContext gameDbContext, ILogger<AuthenticationService> logger)
        {
            this.anyMMOWebServerSettings = anyMMOWebServerSettings;
            this.gameDbContext = gameDbContext;
            this.logger = logger;
        }

        public (bool success, string content) AddUserFromForm(IFormCollection collection)
        {

            // put form into variables
            string userName = collection["UserName"].ToString();
            string password = collection["Password"].ToString();
            string email = collection["Email"].ToString();
            string phone = collection["Phone"].ToString();

            // format a user object and return the value of the registration attempt
            User user = new User()
            {
                UserName = userName,
                Email = email,
                Phone = phone,
                PasswordHash = password
            };

            return Register(user);
        }

        public (bool success, string content) Register(User user)
        {
            // check if username is not taken
            if (gameDbContext.Users.Any(u => u.UserName == user.UserName))
            {
                logger.LogInformation($"Username {user.UserName} was not available during registration attempt");
                return (false, "Username not available");
            }
            
            // populate salt and hash values
            user.ProvideSaltAndHash();

            gameDbContext.Add(user);
            gameDbContext.SaveChanges();
            logger.LogInformation($"Registered new user {user.UserName}");

            return (true, "");

        }

        public (bool success, AuthenticationResponse? authenticationResponse) Login(AuthenticationRequest authenticationRequest, HttpContext httpContext)
        {
            IPAddress? remoteIpAddress = httpContext.Connection.RemoteIpAddress;
            string ipAddress = remoteIpAddress != null ? remoteIpAddress.ToString() : "Unknown";
            var user = gameDbContext.Users.SingleOrDefault(u => u.UserName == authenticationRequest.UserName);
            if (user == null)
            {
                logger.LogInformation($"[LOGIN] invalid username {authenticationRequest.UserName} from IP {ipAddress}");
                return (false, null);
            }

            if (user.PasswordHash != AuthenticationHelpers.ComputeHash(authenticationRequest.Password, user.Salt))
            {
                logger.LogInformation($"[LOGIN] invalid password for user {authenticationRequest.UserName} from IP {ipAddress}");
                return (false, null);
            }

            var task = CookieLogin(user, httpContext);
            task.Wait();

            logger.LogInformation($"[LOGIN] Successfully logged in user {authenticationRequest.UserName} from IP {ipAddress}");
            AuthenticationResponse _authenticationResponse = new AuthenticationResponse() { AccountId = user.Id };
            _authenticationResponse.Token = GenerateJwtToken(AssembleClaimsIdentity(user));
            return (true, _authenticationResponse);
        }

        public (bool success, string token) ServerLogin(ServerAuthenticationRequest authenticationRequest, HttpContext httpContext) {

            IPAddress? remoteIpAddress = httpContext.Connection.RemoteIpAddress;
            string ipAddress = remoteIpAddress != null ? remoteIpAddress.ToString() : "Unknown";

            if (authenticationRequest.SharedSecret != anyMMOWebServerSettings.SharedSecret) {
                logger.LogInformation($"[LOGIN] invalid shared secret for server from IP {ipAddress}");
                return (false, "Invalid password");
            }

            logger.LogInformation($"[LOGIN] Successfully logged in server from IP {ipAddress}");

            return (true, GenerateJwtToken(AssembleClaimsIdentity(authenticationRequest.SharedSecret)));
        }

        public void Logout(HttpContext httpContext)
        {
            var task = httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            task.Wait();
            logger.LogInformation($"[LOGOUT] Logged out user");
        }

        private async Task CookieLogin(User user, HttpContext httpContext)
        {
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim("Id", user.Id.ToString())
                };
            var claimsIdentity = new ClaimsIdentity(claims, "Login");

            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
        }

        private ClaimsIdentity AssembleClaimsIdentity(User user)
        {
            var subject = new ClaimsIdentity(new[]{
                new Claim("id", user.Id.ToString()),
            });

            return subject;
        }

        private ClaimsIdentity AssembleClaimsIdentity(string sharedSecret) {
            var subject = new ClaimsIdentity(new[]{
                new Claim("sharedSecret", sharedSecret),
            });

            return subject;
        }

        private string GenerateJwtToken(ClaimsIdentity subject)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(anyMMOWebServerSettings.BearerKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = subject,
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public (bool success, string errorMessage) ChangePassword(int userId, string oldPassword, string newPassword) {
            var user = gameDbContext.Users.Find(userId);
            if (user == null) {
                return (false, "User not found.");
            }

            // 1. Verify old password using your existing ComputeHash logic
            string hashedOldPassword = AuthenticationHelpers.ComputeHash(oldPassword, user.Salt);
            if (user.PasswordHash != hashedOldPassword) {
                logger.LogInformation($"[PASSWORD CHANGE] Failed for user {user.UserName}: Incorrect old password.");
                return (false, "Current password is incorrect.");
            }

            // 2. Update password
            // Using your model's existing logic to generate a new salt and hash
            user.PasswordHash = newPassword; // Assuming ProvideSaltAndHash reads from the Password property
            user.ProvideSaltAndHash();

            gameDbContext.Update(user);
            gameDbContext.SaveChanges();

            logger.LogInformation($"[PASSWORD CHANGE] Success for user {user.UserName}");
            return (true, string.Empty);
        }


    }



    public interface IAuthenticationService
    {
        (bool success, string content) Register(User user);
        (bool success, AuthenticationResponse? authenticationResponse) Login(AuthenticationRequest authenticationRequest, HttpContext httpContext);
        (bool success, string token) ServerLogin(ServerAuthenticationRequest authenticationRequest, HttpContext httpContext);
        (bool success, string content) AddUserFromForm(IFormCollection collection);
        void Logout(HttpContext httpContext);
        (bool success, string errorMessage) ChangePassword(int userId, string oldPassword, string newPassword);
    }

    public static class AuthenticationHelpers
    {
        public static void ProvideSaltAndHash(this User user)
        {
            var salt = GenerateSalt();
            user.Salt = Convert.ToBase64String(salt);
            user.PasswordHash = ComputeHash(user.PasswordHash, user.Salt);
        }

        private static byte[] GenerateSalt()
        {
            var rng = RandomNumberGenerator.Create();
            var salt = new byte[24];
            rng.GetBytes(salt);
            return salt;
        }

        public static string ComputeHash(string password, string saltString)
        {
            var salt = Convert.FromBase64String(saltString);

            using var hashGenerator = new Rfc2898DeriveBytes(
                    password,
                    salt,
                    iterations: 600000,
                    HashAlgorithmName.SHA256);

            var bytes = hashGenerator.GetBytes(32); // 32 bytes for SHA-256

            return Convert.ToBase64String(bytes);
        }
    }
}
