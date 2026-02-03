using AnyMMOWebServer.Database;
using AnyMMOWebServer.Models;
using Microsoft.EntityFrameworkCore;

namespace AnyMMOWebServer.Services
{
    public class ItemInstanceService
    {
        private GameDbContext dbContext;
        private ILogger logger;
        private IHttpContextAccessor httpContextAccessor;


		public ItemInstanceService(GameDbContext dbContext, ILogger logger, IHttpContextAccessor httpContextAccessor)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }

        public bool AddItemInstance(ItemInstance itemInstance)
        {
            dbContext.ItemInstances.Add(itemInstance);
            dbContext.SaveChanges();

            logger.LogInformation($"[{DateTime.UtcNow:u}] [{(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress == null ? "Unknown" : httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString())}] Added ItemInstance with Id {itemInstance.Id}");

            return true;
        }

        public (bool, ItemInstance) AddItemInstance(CreateItemInstanceRequest createItemInstanceRequest)
        {
            logger.LogInformation($"[{DateTime.UtcNow:u}] [{(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress == null ? "Unknown" : httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString())}] Adding item instance with ItemInstanceId: {createItemInstanceRequest.ItemInstanceId}");

            ItemInstance itemInstance = new ItemInstance() {
                ItemInstanceId = createItemInstanceRequest.ItemInstanceId,
                SaveData = createItemInstanceRequest.SaveData
            };

            return (AddItemInstance(itemInstance), itemInstance);
        }

        public bool SaveItemInstance(SaveItemInstanceRequest saveItemInstanceRequest)
        {
            logger.LogInformation($"[{DateTime.UtcNow:u}] [{(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress == null ? "Unknown" : httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString())}] Saving item instance with Id: {saveItemInstanceRequest.ItemInstanceId}");

            var itemInstance = dbContext.ItemInstances.First(u => u.ItemInstanceId == saveItemInstanceRequest.ItemInstanceId);
            itemInstance.SaveData = saveItemInstanceRequest.SaveData;
            dbContext.SaveChanges();

            return true;
        }

        public bool DeleteItemInstance(DeleteItemInstanceRequest deleteItemInstanceRequest)
        {
            logger.LogInformation($"[{DateTime.UtcNow:u}] [{(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress == null ? "Unknown" : httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString())}] Deleting item instance with Id: {deleteItemInstanceRequest.ItemInstanceId}");

            var itemInstance = dbContext.ItemInstances.First(u => u.ItemInstanceId == deleteItemInstanceRequest.ItemInstanceId);
            dbContext.ItemInstances.Remove(itemInstance);
            dbContext.SaveChanges();

            return true;
        }

        public ItemInstanceListResponse GetItemInstances()
        {
            logger.LogInformation($"[{DateTime.UtcNow:u}] [{(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress == null ? "Unknown" : httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString())}] Getting list of item instances");

            ItemInstanceListResponse itemInstanceListResponse = new ItemInstanceListResponse()
            {
                ItemInstances = dbContext.ItemInstances.ToList()
            };

            return itemInstanceListResponse;
        }

    }

    
}
