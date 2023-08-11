using CommunityServerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CommunityServerAPI.Repositories;

public class BannedWeaponRepository : IRepository<BannedWeapon, string>
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
        => await _context.BannedWeapons.AnyAsync(w => w.Name == weaponName);

    public async Task<BannedWeapon?> FindAsync(string weaponName)
        => await _context.BannedWeapons.FirstOrDefaultAsync(w => w.Name == weaponName);
}