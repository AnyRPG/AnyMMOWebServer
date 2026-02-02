using AnyMMOWebServer.Database;
using AnyMMOWebServer.Models;
using Microsoft.EntityFrameworkCore;

namespace AnyMMOWebServer.Services {
    public class FriendListService {
        private GameDbContext dbContext;
        private ILogger logger;

        public FriendListService(GameDbContext dbContext, ILogger logger) {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public bool AddFriendList(FriendList friendList) {
            dbContext.FriendLists.Add(friendList);
            dbContext.SaveChanges();

            logger.LogInformation($"Added Friend List with Id {friendList.Id}");

            return true;
        }

        public (bool, FriendList) AddFriendList(CreateFriendListRequest createFriendListRequest) {

            logger.LogInformation($"Adding friend list for player character with Id: {createFriendListRequest.PlayerCharacterId}");

            FriendList friendList = new FriendList() {
                PlayerCharacterId = createFriendListRequest.PlayerCharacterId,
                SaveData = createFriendListRequest.SaveData
            };

            return (AddFriendList(friendList), friendList);
        }

        public bool SaveFriendList(SaveFriendListRequest saveFriendListRequest) {
            logger.LogInformation($"Saving friend list for player character with Id: {saveFriendListRequest.PlayerCharacterId}");

            var friendList = dbContext.FriendLists.First(u => u.PlayerCharacterId == saveFriendListRequest.PlayerCharacterId);
            friendList.SaveData = saveFriendListRequest.SaveData;
            dbContext.SaveChanges();

            return true;
        }

        public bool DeleteFriendList(DeleteFriendListRequest deleteFriendListRequest) {
            logger.LogInformation($"Deleting friend list for player character with Id: {deleteFriendListRequest.PlayerCharacterId}");

            var friendList = dbContext.FriendLists.First(u => u.PlayerCharacterId == deleteFriendListRequest.PlayerCharacterId);
            dbContext.FriendLists.Remove(friendList);
            dbContext.SaveChanges();

            return true;
        }

        public FriendListResponse GetFriendLists() {
            logger.LogInformation($"Getting all friend lists");

            FriendListResponse friendListListResponse = new FriendListResponse() {
                FriendLists = dbContext.FriendLists.ToList()
            };

            return friendListListResponse;
        }

    }


}
