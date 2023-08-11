using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using System.Threading.Channels;
using CommunityServerAPI.Models;
using CommunityServerAPI.Repositories;

class Program
{
    public static PlayerRepository Player;
    public static BannedWeaponRepository BannedWeapons;

    static void Main(string[] args)
    {
        // Database connection
        using (var dbContext = new DatabaseContext())
        {
            // Initiate repositories
            Player = new PlayerRepository(dbContext);
            BannedWeapons = new BannedWeaponRepository(dbContext);
        }

        var listener = new ServerListener<MyPlayer, MyGameServer>();
        listener.Start(29294);

        Thread.Sleep(-1);
    }
}

class MyPlayer : Player<MyPlayer>
{
}

class MyGameServer : GameServer<MyPlayer>
{
    public override async Task<bool> OnPlayerTypedMessage(MyPlayer author, ChatChannel channel, string message)
    {
        // Here we make commands like "!banweapon M4A1" etc. to ban and unban weapons.
        // These commands use our repository to put and remove them from the database.
        // returning true means putting the message in chat, false for not putting it in chat.

        if (author.SteamID != 123456789 || !message.StartsWith("!")) return true; // Whatever checks you want to do.

        var words = message.Split(" ");
        switch (words[0])
        {
            case "!banweapon":
                if (!await Program.BannedWeapons.ExistsAsync(words[1]))
                {
                    await Program.BannedWeapons.CreateAsync(new BannedWeapon { Name = words[1] });
                }
                break;

            case "!unbanweapon":
                if (await Program.BannedWeapons.ExistsAsync(words[1]))
                {
                    await Program.BannedWeapons.DeleteAsync(new BannedWeapon { Name = words[1] });
                }

                break;
        }
        
        return false;
    }
    
    public override async Task OnSavePlayerStats(ulong steamId, PlayerStats stats)
    {
        // Check if there's already an entry in the DB, if so, update it, otherwise, create one.
        var player = new ServerPlayer { steamId = steamId, stats = stats };
        if (await Program.Player.ExistsAsync(steamId))
        {
            await Program.Player.UpdateAsync(player);
        }
        else
        {
            await Program.Player.CreateAsync(player);
        }
    }

    public override async Task<PlayerStats> OnGetPlayerStats(ulong steamId, PlayerStats officialStats)
        // Here we try to get the player out of the database. Return a new PlayersStats() if null, otherwise
        // we will put player in a variable and return its stats.
        => await Program.Player.FindAsync(steamId) switch
        {
            null => new PlayerStats(),
            var player => player.stats
        };

    public override async Task<OnPlayerSpawnArguments> OnPlayerSpawning(MyPlayer player, OnPlayerSpawnArguments request)
    {
        // Check if the it's in the banned weapons table, if so, we don't allow it.
        if (await Program.BannedWeapons.ExistsAsync(request.Loadout.PrimaryWeapon.Tool.Name))
            request.Loadout.PrimaryWeapon.Tool = null;

        return request;
    }
}