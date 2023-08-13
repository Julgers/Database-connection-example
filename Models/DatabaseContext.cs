using BattleBitAPI.Common;
using Microsoft.EntityFrameworkCore;

namespace CommunityServerAPI.Models;

/*
 *  In inplementations of DbContext we define all entities that we want to have included in the database.
 *  These will be converted into tables. We can also do additional configuration that we didn't put in the entity classes (models) yet.
 */
public class DatabaseContext : DbContext
{
    // These are your database tables
    public DbSet<ServerPlayer> Player { get; set; }
    public DbSet<BannedWeapon> BannedWeapons { get; set; }

    // Oki made a nice conversion to serialise to byte array for us, and create a new playerstats object out of a byte array.
    // Here we tell EF that it has to convert the "stats" property to byte array whenever storing it inside the DB, and to
    // make a new playerstats object using the blob from the DB whenever pulling it from the DB.
    // If EF didn't do this for us, we would have to manually serialize to byte[] every time we write to DB
    // and manually create a new PlayerStats from the byte array in the DB every time we pull from the db.
    // Storing the full struct in DB instead of a blob is not at all convenient or necessary for our use case.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ServerPlayer>()
            .Property(p => p.stats)
            .HasConversion(
                s => s.SerializeToByteArray(),
                s => new PlayerStats(s));
    }
    
    // Do not store your connection string in your code like here. Use environment variables etc. instead.
    // Never commit secrets (like your db connection string) to version control, like git. Especially in public repositories.
    // This is here just because it's an example and it's a local testing database that we do not care about.
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        const string dbConnectionString = "server=localhost;port=3306;database=commapi;user=root;password=";
        optionsBuilder.UseMySql(dbConnectionString, ServerVersion.AutoDetect(dbConnectionString));
    }
}