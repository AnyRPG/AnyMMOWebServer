using AnyMMOWebServer.Database;
using AnyMMOWebServer.Models;
using Microsoft.EntityFrameworkCore;

namespace AnyMMOWebServer.Services {
    public class MailMessageService {
        private GameDbContext dbContext;
        private ILogger logger;
        private IHttpContextAccessor httpContextAccessor;

        public MailMessageService(GameDbContext dbContext, ILogger logger, IHttpContextAccessor httpContextAccessor) {
            this.dbContext = dbContext;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }

        public bool AddMailMessage(MailMessage mailMessage) {
            dbContext.MailMessages.Add(mailMessage);
            dbContext.SaveChanges();

            logger.LogInformation($"[{DateTime.UtcNow:u}] [{(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress == null ? "Unknown" : httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString())}] Added Mail Message with Id {mailMessage.Id}");

            return true;
        }

        public (bool, MailMessage) AddMailMessage(CreateMailMessageRequest createMailMessageRequest) {

            MailMessage mailMessage = new MailMessage() {
                PlayerCharacterId = createMailMessageRequest.PlayerCharacterId,
                SaveData = createMailMessageRequest.SaveData
            };

            return (AddMailMessage(mailMessage), mailMessage);
        }

        public bool SaveMailMessage(SaveMailMessageRequest saveMailMessageRequest) {
            logger.LogInformation($"[{DateTime.UtcNow:u}] [{(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress == null ? "Unknown" : httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString())}] Saving Mail Message with Id {saveMailMessageRequest.Id}");

            var mailMessage = dbContext.MailMessages.First(u => u.Id == saveMailMessageRequest.Id);
            mailMessage.SaveData = saveMailMessageRequest.SaveData;
            dbContext.SaveChanges();

            return true;
        }

        public bool DeleteMailMessage(DeleteMailMessageRequest deleteMailMessageRequest) {
            logger.LogInformation($"[{DateTime.UtcNow:u}] [{(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress == null ? "Unknown" : httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString())}] Deleting Mail Message with Id {deleteMailMessageRequest.Id}");

            MailMessage? mailMessage = dbContext.MailMessages.FirstOrDefault(u => u.Id == deleteMailMessageRequest.Id);
            if (mailMessage == null) {
                return false;
            }

            dbContext.MailMessages.Remove(mailMessage);
            dbContext.SaveChanges();

            return true;
        }

        public MailMessageListResponse GetMailMessages(int playerCharacterId) {
            logger.LogInformation($"[{DateTime.UtcNow:u}] [{(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress == null ? "Unknown" : httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString())}] Getting all mail messages for player character Id {playerCharacterId}");

            MailMessageListResponse mailMessageListResponse = new MailMessageListResponse() {
                MailMessages = dbContext.MailMessages.Where(u => u.PlayerCharacterId == playerCharacterId).ToList()
            };

            return mailMessageListResponse;
        }

        public MailMessage? GetMailMessage(int messageId) {
            logger.LogInformation($"[{DateTime.UtcNow:u}] [{(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress == null ? "Unknown" : httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString())}] Getting mail message with Id {messageId}");

            MailMessage? mailMessage = dbContext.MailMessages.FirstOrDefault(u => u.Id == messageId);

            return mailMessage;
        }


    }


}
