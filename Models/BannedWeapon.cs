using Microsoft.EntityFrameworkCore;

namespace CommunityServerAPI.Models;

[PrimaryKey(nameof(Name))]
public class BannedWeapon
{
    public string Name;
}