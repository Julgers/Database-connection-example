using System.ComponentModel.DataAnnotations.Schema;
using BattleBitAPI.Common;
using Microsoft.EntityFrameworkCore;

namespace DatabaseExample.Models;

[PrimaryKey(nameof(SteamId))] // Defines the primary key
public class ServerPlayer
{
    // Remove auto increment etc.
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ulong SteamId { get; set; }

    public PlayerStats Stats { get; set; }
}