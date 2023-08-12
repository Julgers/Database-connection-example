using CommunityServerAPI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CommunityServerAPI;

public sealed class InteractableServiceProvider
{
    public static readonly IHostBuilder Builder = CreateDefaultBuilder();
    public static IServiceScopeFactory Services { get; set; }
    
    private static IHostBuilder CreateDefaultBuilder()
        => Host.CreateDefaultBuilder().ConfigureServices(services =>
        {
            services.AddLogging();
            
            services.AddHostedService<ListenerService>();
        });

}