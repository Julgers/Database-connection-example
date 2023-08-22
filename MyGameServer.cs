using BattleBitAPI.Common;
using BattleBitAPI.Server;
using DatabaseExample.Models;
using DatabaseExample.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseExample;

internal class MyGameServer : GameServer<MyPlayer>
{
    private readonly IServiceProvider _services;

    public MyGameServer(IServiceProvider services)
    {
        _services = services;
    }

    public override async Task<bool> OnPlayerTypedMessage(MyPlayer author, ChatChannel channel, string message)
    {
        // Here we make commands like "!banweapon M4A1" etc. to ban and unban weapons.
        // These commands use our repository to put and remove them from the database.
        // returning true means putting the message in chat, false for not putting it in chat.

        if (author.SteamID != 76561198173566107 || !message.StartsWith("!"))
            return true; // Whatever checks you want to do.

        var words = message.Split(" ");

        using var scope = _services.CreateScope();
        var bannedWeapons = scope.ServiceProvider.GetRequiredService<BannedWeaponRepository>();

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
        using var scope = _services.CreateScope();
        var players = scope.ServiceProvider.GetRequiredService<PlayerRepository>();

        var player = new ServerPlayer { SteamId = steamId, Stats = stats };

        // Check if there's already an entry in the DB, if so, update it, otherwise, create one.
        if (await players.ExistsAsync(steamId))
            await players.UpdateAsync(player);
        else
            await players.CreateAsync(player);
    }

    public override async Task OnPlayerJoiningToServer(ulong steamId, PlayerJoiningArguments args)
    {
        using var scope = _services.CreateScope();
        var players = scope.ServiceProvider.GetRequiredService<PlayerRepository>();

        // Here we try to get the player out of the database. Return a new PlayersStats() if null, otherwise
        // we will put player in a variable and return its stats.
        args.Stats = await players.FindAsync(steamId) switch
        {
            null => new PlayerStats(),
            var player => player.Stats
        };
    }

    public override async Task<OnPlayerSpawnArguments?> OnPlayerSpawning(MyPlayer player,
        OnPlayerSpawnArguments request)
    {
        using var scope = _services.CreateScope();
        var bannedWeapons = scope.ServiceProvider.GetRequiredService<BannedWeaponRepository>();

        // Check if the it's in the banned weapons table, if so, we don't allow it.
        if (await bannedWeapons.ExistsAsync(request.Loadout.PrimaryWeapon.Tool.Name))
        {
            player.Message($"Cannot use banned weapon {request.Loadout.PrimaryWeapon.Tool.Name}!", 1f);
            return null; // Deny spawn request.
        }

        return request;
    }
}