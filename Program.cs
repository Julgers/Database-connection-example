using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using CommunityServerAPI.Models;
using CommunityServerAPI.Repositories;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static void Main(string[] args)
    {
        var listener = new ServerListener<MyPlayer, MyGameServer>();

        // Auto apply any pending migrations on startup.
        using (var context = new DatabaseContext())
        {
            context.Database.Migrate();
        }

        listener.Start(29294);

        Thread.Sleep(-1);
    }
}

internal class MyPlayer : Player<MyPlayer>
{
}

internal class MyGameServer : GameServer<MyPlayer>
{
    public override async Task OnConnected()
    {
        Console.WriteLine($"Gameserver connected! {GameIP}:{GamePort}");
    }

    public override async Task<bool> OnPlayerTypedMessage(MyPlayer author, ChatChannel channel, string message)
    {
        // Here we make commands like "!banweapon M4A1" etc. to ban and unban weapons.
        // These commands use our repository to put and remove them from the database.
        // returning true means putting the message in chat, false for not putting it in chat.

        if (author.SteamID != 76561198173566107 || !message.StartsWith("!"))
            return true; // Whatever checks you want to do.

        var words = message.Split(" ");

        // `Using` makes sure it gets disposed correctly (when the variable falls out of scope).
        // We need to await it to make sure everything is finished before disposing.
        await using var bannedWeapons = new BannedWeaponRepository();

        switch (words[0])
        {
            case "!banweapon":
                if (!await bannedWeapons.ExistsAsync(words[1]))
                    await bannedWeapons.CreateAsync(new BannedWeapon { Name = words[1] });
                break;

            case "!unbanweapon":
                if (await bannedWeapons.ExistsAsync(words[1]))
                    await bannedWeapons.DeleteAsync(new BannedWeapon { Name = words[1] });

                break;
        }

        return false;
    }

    public override async Task OnSavePlayerStats(ulong steamId, PlayerStats stats)
    {
        var player = new ServerPlayer { steamId = steamId, stats = stats };
        // Check if there's already an entry in the DB, if so, update it, otherwise, create one.

        // `Using` makes sure it gets disposed correctly (when the variable falls out of scope).
        // We need to await it to make sure everything is finished before disposing.
        await using var players = new PlayerRepository();

        if (await players.ExistsAsync(steamId))
            await players.UpdateAsync(player);
        else
            await players.CreateAsync(player);
    }

    public override async Task OnPlayerJoiningToServer(ulong steamId, PlayerJoiningArguments args)
        // Here we try to get the player out of the database. Return a new PlayersStats() if null, otherwise
        // we will put player in a variable and return its stats.
    {
        // `Using` makes sure it gets disposed correctly (when the variable falls out of scope).
        // We need to await it to make sure everything is finished before disposing.
        await using var players = new PlayerRepository();

        args.Stats = await players.FindAsync(steamId) switch
        {
            null => new PlayerStats(),
            var player => player.stats
        };
    }

    public override async Task<OnPlayerSpawnArguments> OnPlayerSpawning(MyPlayer player, OnPlayerSpawnArguments request)
    {
        // Check if the it's in the banned weapons table, if so, we don't allow it.

        // `Using` makes sure it gets disposed correctly (when the variable falls out of scope).
        // We need to await it to make sure everything is finished before disposing.
        await using var bannedWeapons = new BannedWeaponRepository();

        if (await bannedWeapons.ExistsAsync(request.Loadout.PrimaryWeapon.Tool.Name))
            request.Loadout.PrimaryWeapon.Tool = default;

        return request;
    }
}