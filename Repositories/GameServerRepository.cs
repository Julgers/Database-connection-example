using System.Net;
using DatabaseExample.Models;
using Microsoft.EntityFrameworkCore;

namespace DatabaseExample.Repositories;

// Reminder that we use a composite key for this table
public sealed class GameServerRepository : IRepository<GameServer, (IPAddress ip, ushort port, string token)>
{
    private readonly DatabaseContext _context;

    public GameServerRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(GameServer server)
    {
        _context.GameServers.Add(server);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(GameServer server)
    {
        _context.GameServers.Remove(server);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(GameServer server)
    {
        _context.GameServers.Update(server);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync((IPAddress ip, ushort port, string token) server)
    {
        return await _context.GameServers.AnyAsync(
            s => server.ip == s.Ip && server.port == s.Port && server.token == s.Token);
    }

    public async Task<GameServer?> FindAsync((IPAddress ip, ushort port, string token) server)
    {
        return await _context.GameServers.FirstOrDefaultAsync(
            s => server.ip == s.Ip && server.port == s.Port && server.token == s.Token);
    }
}