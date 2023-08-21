using System.Net;
using BattleBitAPI.Server;
using DatabaseExample.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LogLevel = BattleBitAPI.Common.LogLevel;

namespace DatabaseExample;

public class ListenerService : IHostedService, IDisposable
{
    private readonly ServerListener<MyPlayer, MyGameServer> _listener;
    private readonly ILogger<ListenerService> _logger;
    private readonly IServiceProvider _services;

    public ListenerService(ILogger<ListenerService> logger, IServiceProvider services)
    {
        _services = services;

        _listener = new ServerListener<MyPlayer, MyGameServer>();
        _listener.LogLevel = LogLevel.All;
        _listener.OnValidateGameServerToken += OnValidateGameToken;
        _listener.OnCreatingGameServerInstance += OnCreatingGameServerInstance;
        _listener.OnCreatingPlayerInstance += OnCreatingPlayerInstance;
        _listener.OnLog += OnLog;

        _logger = logger;
    }

    private void OnLog(LogLevel lvl, string msg, object obj)
    {
        _logger.LogInformation($"Log (level {lvl}): {msg}");
    }

    private MyPlayer OnCreatingPlayerInstance(ulong arg)
    {
        return new MyPlayer();
    }

    private MyGameServer OnCreatingGameServerInstance(IPAddress arg1, ushort arg2)
    {
        return new MyGameServer(_services);
    }

    private async Task<bool> OnValidateGameToken(IPAddress ip, ushort port, string token)
    {
        // We have a "whitelist" of IP + port + token entries in database.
        // Validate() checks if the DB has any entries with exactly this ip, port and token.
        var gameServers = _services.GetRequiredService<GameServerRepository>();
        return await gameServers.Validate(ip, port, token);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _listener.Start(IPAddress.Loopback, 29294);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _listener.Stop();

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _listener.Dispose();

        GC.SuppressFinalize(this);
    }
}