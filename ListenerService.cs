using System.Net;
using BattleBitAPI.Server;
using DatabaseExample.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LogLevel = BattleBitAPI.Common.LogLevel;

namespace DatabaseExample;

public sealed class ListenerService : IHostedService, IDisposable
{
    /*
     * Since this is a hosted service, we can receive things like the IServiceProvider and ILogger in our constructor
     * to use them here. We want the service provider to create service scopes and resolve our repository services,
     * which is also why we pass it in the constructor of MyPlayer and MyGameServer.
     */

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
        // ExistsAsync checks if the DB has any entries with exactly this composite key of ip, port and token.
        using var scope = _services.CreateScope();
        var gameServers = scope.ServiceProvider.GetRequiredService<GameServerRepository>();
        return await gameServers.ExistsAsync((ip, port, token));
    }

    // Since this is an IHostedservice, here we tell it what to do when starting and (below) stopping the service.
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

    // From IDisposable, make sure we dispose the listener when disposing this service.
    public void Dispose()
    {
        _listener.Dispose();

        GC.SuppressFinalize(this);
    }
}