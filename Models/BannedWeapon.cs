using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DatabaseExample.Models;

[PrimaryKey(nameof(Name))]
public class BannedWeapon
{
    public string Name { get; set; }
}