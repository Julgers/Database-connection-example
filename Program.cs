using DatabaseExample;
using DatabaseExample.Models;
using DatabaseExample.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Manage the configuration
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

/*
 * The order of priority that our application uses as source of configuration is the reverse of what we just did:
 * 1. Command line arguments
 * 2. Environment variables
 * 4. appsettings.json
 */

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services =>
{
    // Add database connection.
    var dbConnectionString = configuration.GetConnectionString("defaultConnection");

    services.AddDbContext<DatabaseContext>(options =>
    {
        options.UseLazyLoadingProxies();
        options.UseMySql(dbConnectionString, ServerVersion.AutoDetect(dbConnectionString));
    }, ServiceLifetime.Transient);
    // ^ We need DatabaseContext to be transient so that we get a new one every time we require it.
    // By default, it is scoped, but the scope is the scope of a player/gameserver, while we want to use DbContext
    // for exactly ONE unit of work / transaction. Hence why we explicitly set its lifetime to transient.

    services.AddLogging();

    // Repositories. Because these require DbContext, we want them to be transient for the same reason.
    // To make sure we have a fresh DbContext every time we require a repository
    services.AddTransient<PlayerRepository>();
    services.AddTransient<BannedWeaponRepository>();
    services.AddTransient<GameServerRepository>();

    // Game server listener
    services.AddHostedService<ListenerService>();
});

var app = builder.Build();

// Auto apply any pending db migrations on startup.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetService<DatabaseContext>();
    context.Database.Migrate();
}

app.Run();