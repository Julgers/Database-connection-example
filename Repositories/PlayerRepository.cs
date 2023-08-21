using DatabaseExample.Models;
using Microsoft.EntityFrameworkCore;

namespace DatabaseExample.Repositories;

public class PlayerRepository : IRepository<ServerPlayer, ulong>
{
    private readonly DatabaseContext _context;

    public PlayerRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(ServerPlayer player)
    {
        _context.Player.Add(player);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ServerPlayer player)
    {
        _context.Player.Remove(player);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ServerPlayer player)
    {
        _context.Player.Update(player);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(ulong steamId)
    {
        return await _context.Player.AnyAsync(p => p.SteamId == steamId);
    }

    public async Task<ServerPlayer?> FindAsync(ulong steamId)
    {
        return await _context.Player.FirstOrDefaultAsync(p => p.SteamId == steamId);
    }
}