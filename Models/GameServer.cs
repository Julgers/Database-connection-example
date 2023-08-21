using System.Net;

namespace DatabaseExample.Models;

public class GameServer
{
    public IPAddress Ip { get; set; }
    public ushort Port { get; set; }
    public string Token { get; set; }
}