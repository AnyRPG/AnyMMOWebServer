using Microsoft.EntityFrameworkCore;
using AnyMMOWebServer.Models;

namespace AnyMMOWebServer.Database
{
    public class GameDbContext : DbContext {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) {
        }

        public DbSet<User>? Users { get; set; }
        public DbSet<PlayerCharacter>? PlayerCharacters { get; set; }
    }
}
