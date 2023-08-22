#nullable enable
using DatabaseExample.Models;
using Microsoft.EntityFrameworkCore;

namespace DatabaseExample.Repositories;

public sealed class BannedWeaponRepository : IRepository<BannedWeapon, string>, IDisposable
{
    private readonly DatabaseContext _context;

    public BannedWeaponRepository(DatabaseContext context)
    {
        _context = context;
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
    {
        return await _context.BannedWeapons.AnyAsync(w => w.Name == weaponName);
    }

    public async Task<BannedWeapon?> FindAsync(string weaponName)
    {
        return await _context.BannedWeapons.FirstOrDefaultAsync(w => w.Name == weaponName);
    }

    public void Dispose()
    {
        _context.Dispose();

        GC.SuppressFinalize(this);
    }
}