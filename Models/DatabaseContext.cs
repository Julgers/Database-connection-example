using BattleBitAPI.Common;
using Microsoft.EntityFrameworkCore;

namespace CommunityServerAPI.Models;

/*
 *  In inplementations of DbContext we define all entities that we want to have included in the database.
 *  These will be converted into tables. We can also do additional configuration that we didn't put in the entity classes (models) yet.
 *  This is however not needed in this case.
 */
public class DatabaseContext : DbContext
{
    // These are your database tables
    public DbSet<ServerPlayer> Player { get; set; }
    public DbSet<BannedWeapon> BannedWeapons { get; set; }
    
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    // Oki made a nice conversion to serialise to byte array for us, and create a new playerstats object out of a byte array
    // here we tell EF that it has to convert it to byte array whenever storing it inside the DB, and to
    // make a new playerstats object using the blob from the DB whenever pulling it from the DB.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ServerPlayer>()
            .Property(p => p.stats)
            .HasConversion(
                s => s.SerializeToByteArray(),
                s => new PlayerStats(s));
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        
        const string dbConnectionString = "server=localhost;port=3306;database=commapi;user=root;password=";
        optionsBuilder.UseMySql(dbConnectionString, ServerVersion.AutoDetect(dbConnectionString));
    }
}