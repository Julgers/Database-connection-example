using BattleBitAPI.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommunityServerAPI.Services;

public class ListenerService : IHostedService, IDisposable
{
    private readonly ServerListener<MyPlayer, MyGameServer> _listener;
    private readonly ILogger<ListenerService> _logger;

    public ListenerService(ILogger<ListenerService> logger)
    {
        _listener = new ServerListener<MyPlayer, MyGameServer>();
        
        _logger = logger;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_listener.IsListening)
            _listener.Start(29294);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_listener.IsListening)
            _listener.Stop();
        
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _listener.Dispose();
        
        GC.SuppressFinalize(this);
    }
}