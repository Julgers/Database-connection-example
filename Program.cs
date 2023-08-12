using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using CommunityServerAPI;
using CommunityServerAPI.Models;
using CommunityServerAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

class Program
{
    static void Main(string[] args)
    {
        var builder = InteractableServiceProvider.Builder.ConfigureServices(services =>
        {
            services.AddLogging();
            services.AddDbContext<DatabaseContext>((ServiceProvider, options) =>
            {
                // Allows for automatic inclusion of relations
                options.UseLazyLoadingProxies();
                
                // Here we connect to our database with a connection string.
                // Do not store your connection string in the code like this for production: Use environment variables or some other secret management instead.
                const string dbConnectionString = "server=localhost;port=3306;database=commapi;user=root;password=";
                options.UseMySql(dbConnectionString, ServerVersion.AutoDetect(dbConnectionString));
            });
            
            services.AddScoped<PlayerRepository>();
            services.AddScoped<BannedWeaponRepository>();
        });
        
        var app = builder.Build();
        
        InteractableServiceProvider.Services = app.Services.GetRequiredService<IServiceScopeFactory>();
        
        // Auto migrate on startup
        using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var context = serviceScope.ServiceProvider.GetService<DatabaseContext>();
            context?.Database.Migrate();
        }
        
        app.Run();
    }
}

class MyPlayer : Player<MyPlayer>
{
}

class MyGameServer : GameServer<MyPlayer>
{
    private readonly PlayerRepository _player;
    private readonly BannedWeaponRepository _bannedWeapon;

    public MyGameServer()
    {
        using (var scope = InteractableServiceProvider.Services.CreateScope())
        {
            _player = scope.ServiceProvider.GetService<PlayerRepository>();
            _bannedWeapon = scope.ServiceProvider.GetService<BannedWeaponRepository>();
        }
    }
    
    public override async Task OnConnected()
    {
        Console.WriteLine($"Gameserver connected! {this.GameIP}:{this.GamePort}");
    }

    public override async Task<bool> OnPlayerTypedMessage(MyPlayer author, ChatChannel channel, string message)
    {
        // Here we make commands like "!banweapon M4A1" etc. to ban and unban weapons.
        // These commands use our repository to put and remove them from the database.
        // returning true means putting the message in chat, false for not putting it in chat.

        if (author.SteamID != 76561198173566107 || !message.StartsWith("!")) return true; // Whatever checks you want to do.

        var words = message.Split(" ");
        switch (words[0])
            {
                case "!banweapon":
                    if (!await _bannedWeapon.ExistsAsync(words[1]))
                    {
                        await _bannedWeapon.CreateAsync(new BannedWeapon { Name = words[1] });
                    }
                    break;

                case "!unbanweapon":
                    if (await _bannedWeapon.ExistsAsync(words[1]))
                    {
                        await _bannedWeapon.DeleteAsync(new BannedWeapon { Name = words[1] });
                    }

                    break;
            }

        return false;
    }
    
    public override async Task OnSavePlayerStats(ulong steamId, PlayerStats stats)
    {
        var player = new ServerPlayer { steamId = steamId, stats = stats };
        // Check if there's already an entry in the DB, if so, update it, otherwise, create one.

        if (await _player.ExistsAsync(steamId))
        {
            await _player.UpdateAsync(player);
        }
        else
        {
            await _player.CreateAsync(player);
        }
    }

    public override async Task<PlayerStats> OnGetPlayerStats(ulong steamId, PlayerStats officialStats)
        // Here we try to get the player out of the database. Return a new PlayersStats() if null, otherwise
        // we will put player in a variable and return its stats.
    {
        return await _player.FindAsync(steamId) switch
        {
            null => new PlayerStats(),
            var player => player.stats
        };
    }
    
    public override async Task<OnPlayerSpawnArguments> OnPlayerSpawning(MyPlayer player, OnPlayerSpawnArguments request)
    {
        // Check if the it's in the banned weapons table, if so, we don't allow it.
        if (await _bannedWeapon.ExistsAsync(request.Loadout.PrimaryWeapon.Tool.Name))
            request.Loadout.PrimaryWeapon.Tool = null;

        return request;
    }
}