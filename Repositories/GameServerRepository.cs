using System.Net;
using DatabaseExample.Models;
using Microsoft.EntityFrameworkCore;

namespace DatabaseExample.Repositories;

public class GameServerRepository
{
    private readonly DatabaseContext _context;

    public GameServerRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<bool> Validate(IPAddress ipAddress, ushort port, string token)
    {
        return await _context.GameServers.AnyAsync(
            server => server.Ip == ipAddress && server.Port == port && server.Token == token);
    }
}