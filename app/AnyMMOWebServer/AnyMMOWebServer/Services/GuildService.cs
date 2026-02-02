using AnyMMOWebServer.Database;
using AnyMMOWebServer.Models;
using Microsoft.EntityFrameworkCore;

namespace AnyMMOWebServer.Services
{
    public class GuildService
    {
        private GameDbContext dbContext;
        private ILogger logger;

        public GuildService(GameDbContext dbContext, ILogger logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public bool AddGuild(Guild guild)
        {
            //dbContext.Add(user);
            dbContext.Guilds.Add(guild);
            dbContext.SaveChanges();

            logger.LogInformation($"Added Guild with Id {guild.Id}");

            return true;
        }

        public (bool, Guild) AddGuild(CreateGuildRequest createGuildRequest)
        {
            logger.LogInformation($"Adding new guild");

            Guild guild = new Guild()
            {
                SaveData = createGuildRequest.SaveData
            };

            return (AddGuild(guild), guild);
        }

        public bool SaveGuild(SaveGuildRequest saveGuildRequest)
        {
            logger.LogInformation($"Saving guild with Id: {saveGuildRequest.Id}");

            var guild = dbContext.Guilds.First(u => u.Id == saveGuildRequest.Id);
            guild.SaveData = saveGuildRequest.SaveData;
            dbContext.SaveChanges();

            return true;
        }

        public bool DeleteGuild(DeleteGuildRequest deleteGuildRequest)
        {
            logger.LogInformation($"Deleting guild with Id: {deleteGuildRequest.Id}");

            var guild = dbContext.Guilds.First(u => u.Id == deleteGuildRequest.Id);
            dbContext.Guilds.Remove(guild);
            dbContext.SaveChanges();

            return true;
        }

        public GuildListResponse GetGuilds()
        {
            logger.LogInformation($"Getting list of guilds");

            GuildListResponse guildListResponse = new GuildListResponse()
            {
                Guilds = dbContext.Guilds.ToList()
            };

            return guildListResponse;
        }

    }

    
}
