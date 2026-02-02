using AnyMMOWebServer.Database;
using AnyMMOWebServer.Models;
using Microsoft.EntityFrameworkCore;

namespace AnyMMOWebServer.Services
{
    public class PlayerCharacterService
    {
        private GameDbContext dbContext;
        private ILogger logger;

        public PlayerCharacterService(GameDbContext dbContext, ILogger logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public bool AddPlayerCharacter(PlayerCharacter playerCharacter)
        {

            if (dbContext.PlayerCharacters.Any(u => u.Name == playerCharacter.Name)) {
                // player with this name already exists
                return false;
            }

            dbContext.PlayerCharacters.Add(playerCharacter);
            dbContext.SaveChanges();

            dbContext.FriendLists.Add(new FriendList() { PlayerCharacterId = playerCharacter.Id});
            dbContext.SaveChanges();

            logger.LogInformation($"Added Player Character {playerCharacter.Name} with Id {playerCharacter.Id}");

            return true;
        }

        public (bool, PlayerCharacter) AddPlayerCharacter(CreateCharacterRequest createCharacterRequest)
        {

            // add player character
            PlayerCharacter playerCharacter = new PlayerCharacter()
            {
                AccountId = createCharacterRequest.AccountId,
                Name = createCharacterRequest.Name,
                SaveData = createCharacterRequest.SaveData
            };

            return (AddPlayerCharacter(playerCharacter), playerCharacter);
        }

        public bool SavePlayerCharacter(SaveCharacterRequest saveCharacterRequest)
        {
            logger.LogDebug($"Saving player character with Id: {saveCharacterRequest.Id}");

            var playerCharacter = dbContext.PlayerCharacters.First(u => u.Id == saveCharacterRequest.Id);
            if (playerCharacter.Name != saveCharacterRequest.Name)
            {
                playerCharacter.Name = saveCharacterRequest.Name;
            }
            playerCharacter.SaveData = saveCharacterRequest.SaveData;
            dbContext.SaveChanges();

            return true;
        }

        public bool DeletePlayerCharacter(DeleteCharacterRequest deleteCharacterRequest)
        {
            logger.LogInformation($"Deleting player character with Id: {deleteCharacterRequest.Id}");

            var playerCharacter = dbContext.PlayerCharacters.First(u => u.Id == deleteCharacterRequest.Id);
            dbContext.PlayerCharacters.Remove(playerCharacter);
            dbContext.SaveChanges();

            var friendList = dbContext.FriendLists.First(u => u.PlayerCharacterId == deleteCharacterRequest.Id);
            dbContext.FriendLists.Remove(friendList);
            dbContext.SaveChanges();

            return true;
        }

        public PlayerCharacterListResponse GetPlayerCharacters(int userId)
        {
            logger.LogInformation($"Loading player character list for account {userId}");

            PlayerCharacterListResponse playerCharacterListResponse = new PlayerCharacterListResponse()
            {
                PlayerCharacters = dbContext.PlayerCharacters.Where(u => u.AccountId == userId).ToList()
            };

            return playerCharacterListResponse;
        }

        public PlayerCharacterListResponse GetAllPlayerCharacters() {
            logger.LogInformation($"Loading all player characters");

            PlayerCharacterListResponse playerCharacterListResponse = new PlayerCharacterListResponse() {
                PlayerCharacters = dbContext.PlayerCharacters.ToList()
            };

            return playerCharacterListResponse;
        }

    }


}
