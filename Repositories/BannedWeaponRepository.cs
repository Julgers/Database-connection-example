using CommunityServerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CommunityServerAPI.Repositories;

public class BannedWeaponRepository : IRepository<BannedWeapon, string>, IAsyncDisposable
{
    private readonly DatabaseContext _context;
    
    public BannedWeaponRepository()
    {
        _context = new DatabaseContext();
    }

    public async Task CreateAsync(BannedWeapon weapon)
    {
        _context.BannedWeapons.Add(weapon);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(BannedWeapon weapon)
    {
        _context.BannedWeapons.Remove(weapon);
        await _context.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(BannedWeapon weapon)
    {
        _context.BannedWeapons.Update(weapon);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(string weaponName)
        => await _context.BannedWeapons.AnyAsync(w => w.Name == weaponName);

    public async Task<BannedWeapon?> FindAsync(string weaponName)
        => await _context.BannedWeapons.FirstOrDefaultAsync(w => w.Name == weaponName);
    

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