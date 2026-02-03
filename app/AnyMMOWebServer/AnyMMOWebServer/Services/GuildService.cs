using AnyMMOWebServer.Database;
using AnyMMOWebServer.Models;
using Microsoft.EntityFrameworkCore;

namespace AnyMMOWebServer.Services
{
    public class GuildService
    {
        private GameDbContext dbContext;
        private ILogger logger;
        private IHttpContextAccessor httpContextAccessor;

        public GuildService(GameDbContext dbContext, ILogger logger, IHttpContextAccessor httpContextAccessor)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }

        public bool AddGuild(Guild guild)
        {
            //dbContext.Add(user);
            dbContext.Guilds.Add(guild);
            dbContext.SaveChanges();

            logger.LogInformation($"[{DateTime.UtcNow:u}] [{(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress == null ? "Unknown" : httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString())}] Added Guild with Id {guild.Id}");

            return true;
        }

        public (bool, Guild) AddGuild(CreateGuildRequest createGuildRequest)
        {
            logger.LogInformation($"[{DateTime.UtcNow:u}] [{(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress == null ? "Unknown" : httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString())}] Adding new guild");

            Guild guild = new Guild()
            {
                SaveData = createGuildRequest.SaveData
            };

            return (AddGuild(guild), guild);
        }

        public bool SaveGuild(SaveGuildRequest saveGuildRequest)
        {
            logger.LogInformation($"[{DateTime.UtcNow:u}] [{(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress == null ? "Unknown" : httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString())}] Saving guild with Id: {saveGuildRequest.Id}");

            var guild = dbContext.Guilds.First(u => u.Id == saveGuildRequest.Id);
            guild.SaveData = saveGuildRequest.SaveData;
            dbContext.SaveChanges();

            return true;
        }

        public bool DeleteGuild(DeleteGuildRequest deleteGuildRequest)
        {
            logger.LogInformation($"[{DateTime.UtcNow:u}] [{(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress == null ? "Unknown" : httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString())}] Deleting guild with Id: {deleteGuildRequest.Id}");

            var guild = dbContext.Guilds.First(u => u.Id == deleteGuildRequest.Id);
            dbContext.Guilds.Remove(guild);
            dbContext.SaveChanges();

            return true;
        }

        public GuildListResponse GetGuilds()
        {
            logger.LogInformation($"[{DateTime.UtcNow:u}] [{(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress == null ? "Unknown" : httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString())}] Getting list of guilds");

            GuildListResponse guildListResponse = new GuildListResponse()
            {
                Guilds = dbContext.Guilds.ToList()
            };

            return guildListResponse;
        }

    }

    
}
