using AnyMMOWebServer.Database;
using AnyMMOWebServer.Models;
using Microsoft.EntityFrameworkCore;

namespace AnyMMOWebServer.Services
{
    public class UserAccountService
    {
        private GameDbContext dbContext;
        private ILogger logger;
        private IHttpContextAccessor httpContextAccessor;

        public UserAccountService(GameDbContext dbContext, ILogger logger, IHttpContextAccessor httpContextAccessor)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }

        public bool AddUser(User user)
        {
            dbContext.Add(user);
            dbContext.Users.Add(user);
            dbContext.SaveChanges();

            logger.LogInformation($"[{DateTime.UtcNow:u}] [{(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress == null ? "Unknown" : httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString())}] Added user {user.UserName} with Id {user.Id}");

            return true;
        }

        public void AddUserFromForm(IFormCollection collection)
        {
            // put form into variables
            string userName = collection["UserName"].ToString();
            string password = collection["Password"].ToString();
            string email = collection["Email"].ToString();
            string phone = collection["Phone"].ToString();

            // check if username is not taken

            // do password stuff

            // add user

            User user = new User()
            {
                UserName = userName,
                Email = email,
                Phone = phone,
                PasswordHash = password,
                Salt = "sdfsdfsd"
            };

            AddUser(user);
        }

        public User GetUser(int id) {
            User returnValue = dbContext.Users.First(u => u.Id == id);
            return returnValue;
        }

        public bool SaveUserDetails(User model) {
            // 2. Retrieve the existing user from the database
            // Use the ID from the hidden field in your view
            var userInDb = dbContext.Users.Find(model.Id);

            if (userInDb == null) {
                return false;
            }

            // 3. Update only the allowed fields
            // We skip UserName since it's readonly on the frontend
            userInDb.Email = model.Email;
            userInDb.Phone = model.Phone;

            // 4. Save changes to the database
            dbContext.Update(userInDb);
            dbContext.SaveChanges();

            return true;
        }
    }
}
