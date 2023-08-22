using System.Net;
using Microsoft.EntityFrameworkCore;

namespace DatabaseExample.Models;

// We will have these three as information, and want all 3 of them to match.
// Someone can have multiple ports / tokens per IP, there will be multiple IPs / tokens per port
// And probably even multiple IPs / ports per token, for if you want to re-use the same token
// because of this, we use a composite key for all 3 of them.
[PrimaryKey(nameof(Ip), nameof(Port), nameof(Token))]
public class GameServer
{
    public IPAddress Ip { get; set; }
    public ushort Port { get; set; }
    public string Token { get; set; }
}