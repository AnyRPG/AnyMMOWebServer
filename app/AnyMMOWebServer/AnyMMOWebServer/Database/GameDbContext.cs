using Microsoft.EntityFrameworkCore;
using AnyMMOWebServer.Models;

namespace AnyMMOWebServer.Database
{
    public class GameDbContext : DbContext {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<PlayerCharacter> PlayerCharacters { get; set; }
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<ItemInstance> ItemInstances { get; set; }
        public DbSet<AuctionItem> AuctionItems { get; set; }
        public DbSet<MailMessage> MailMessages { get; set; }
        public DbSet<FriendList> FriendLists { get; set; }
    }
}
