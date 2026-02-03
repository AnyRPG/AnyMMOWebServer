using AnyMMOWebServer.Database;
using AnyMMOWebServer.Models;
using Microsoft.EntityFrameworkCore;

namespace AnyMMOWebServer.Services
{
    public class AuctionItemService
    {
        private GameDbContext dbContext;
        private ILogger logger;
        private IHttpContextAccessor httpContextAccessor;


		public AuctionItemService(GameDbContext dbContext, ILogger logger, IHttpContextAccessor httpContextAccessor)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }

        public bool AddAuctionItem(AuctionItem auctionItem)
        {
            dbContext.AuctionItems.Add(auctionItem);
            dbContext.SaveChanges();

            logger.LogInformation($"[{DateTime.UtcNow:u}] [{(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress == null ? "Unknown" : httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString())}] Added Auction Item with Id {auctionItem.Id}");

            return true;
        }

        public (bool, AuctionItem) AddAuctionItem(CreateAuctionItemRequest createAuctionItemRequest)
        {
            logger.LogInformation($"[{DateTime.UtcNow:u}] [{(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress == null ? "Unknown" : httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString())}] Adding new auction item");

            AuctionItem auctionItem = new AuctionItem()
            {
                SaveData = createAuctionItemRequest.SaveData
            };

            return (AddAuctionItem(auctionItem), auctionItem);
        }

        public bool SaveAuctionItem(SaveAuctionItemRequest saveAuctionItemRequest)
        {
            logger.LogInformation($"[{DateTime.UtcNow:u}] [{(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress == null ? "Unknown" : httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString())}] Saving auction item with Id: {saveAuctionItemRequest.Id}");

            var auctionItem = dbContext.AuctionItems.First(u => u.Id == saveAuctionItemRequest.Id);
            auctionItem.SaveData = saveAuctionItemRequest.SaveData;
            dbContext.SaveChanges();

            return true;
        }

        public bool DeleteAuctionItem(DeleteAuctionItemRequest deleteAuctionItemRequest)
        {
            logger.LogInformation($"[{DateTime.UtcNow:u}] [{(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress == null ? "Unknown" : httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString())}] Deleting auction item with Id: {deleteAuctionItemRequest.Id}");

            var auctionItem = dbContext.AuctionItems.First(u => u.Id == deleteAuctionItemRequest.Id);
            dbContext.AuctionItems.Remove(auctionItem);
            dbContext.SaveChanges();

            return true;
        }

        public AuctionItemListResponse GetAuctionItems()
        {
            logger.LogInformation($"[{DateTime.UtcNow:u}] [{(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress == null ? "Unknown" : httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString())}] Loading all auction items");

            AuctionItemListResponse auctionItemListResponse = new AuctionItemListResponse()
            {
                AuctionItems = dbContext.AuctionItems.ToList()
            };

            return auctionItemListResponse;
        }

    }

    
}
