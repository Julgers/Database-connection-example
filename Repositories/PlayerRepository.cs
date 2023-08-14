using CommunityServerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CommunityServerAPI.Repositories;

public class PlayerRepository : IRepository<ServerPlayer, ulong>, IAsyncDisposable
{
    private readonly DatabaseContext _context;
    
    public PlayerRepository()
    {
        _context = new DatabaseContext();
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
        => await _context.Player.AnyAsync(p => p.steamId == steamId);

    public async Task<ServerPlayer?> FindAsync(ulong steamId)
    {
        return await _context.Player.FirstOrDefaultAsync(p => p.steamId == steamId);
    }
    
    /*
     * DbContext instances are meant to be used for only ONE SQL transaction. We do that transaction and then have to dispose it.
     * With this function and being an IDisposable, we dispose the DatabaseContext when we dispose the repository.
     * Otherwise, we would have to dispose the context in the actual application logic, which would make our abstraction a lot more leaky.
     * Now we just dispose the repository in the application logic. This gives away less storage implementation details.
     * We use DisposeAsync() and implement an IAsyncDisposable because we have to await if the transactions are finished before disposing.
     */
    public async ValueTask DisposeAsync()
    {
        // Makes it harder to misuse the repository class.
        if (_context.Database.CurrentTransaction != null)
            throw new Exception("Disposing DatabaseContext while still in use!");
        
        await _context.DisposeAsync();
        GC.SuppressFinalize(this);
        await Task.CompletedTask;
    }
}