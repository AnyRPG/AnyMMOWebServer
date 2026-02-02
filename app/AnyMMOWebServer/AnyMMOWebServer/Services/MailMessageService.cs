using AnyMMOWebServer.Database;
using AnyMMOWebServer.Models;
using Microsoft.EntityFrameworkCore;

namespace AnyMMOWebServer.Services {
    public class MailMessageService {
        private GameDbContext dbContext;
        private ILogger logger;

        public MailMessageService(GameDbContext dbContext, ILogger logger) {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public bool AddMailMessage(MailMessage mailMessage) {
            dbContext.MailMessages.Add(mailMessage);
            dbContext.SaveChanges();

            logger.LogInformation($"Added Mail Message with Id {mailMessage.Id}");

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
            logger.LogInformation($"Saving Mail Message with Id {saveMailMessageRequest.Id}");

            var mailMessage = dbContext.MailMessages.First(u => u.Id == saveMailMessageRequest.Id);
            mailMessage.SaveData = saveMailMessageRequest.SaveData;
            dbContext.SaveChanges();

            return true;
        }

        public bool DeleteMailMessage(DeleteMailMessageRequest deleteMailMessageRequest) {
            logger.LogInformation($"Deleting Mail Message with Id {deleteMailMessageRequest.Id}");

            var mailMessage = dbContext.MailMessages.First(u => u.Id == deleteMailMessageRequest.Id);
            dbContext.MailMessages.Remove(mailMessage);
            dbContext.SaveChanges();

            return true;
        }

        public MailMessageListResponse GetMailMessages(int playerCharacterId) {
            logger.LogInformation($"Getting all mail messages for player character Id {playerCharacterId}");

            MailMessageListResponse mailMessageListResponse = new MailMessageListResponse() {
                MailMessages = dbContext.MailMessages.Where(u => u.PlayerCharacterId == playerCharacterId).ToList()
            };

            return mailMessageListResponse;
        }

    }


}
