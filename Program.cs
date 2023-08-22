using DatabaseExample;
using DatabaseExample.Models;
using DatabaseExample.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/*
 * This place is basically the entry point of the application, with a nicer syntax.
 * Normally we would be in Main(), this is basically the same thing, to avoid writing boilerplate code.
 */


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


/*
 * First we set up the IoC/DI system: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
 * We now begin setting up and configuring our application class, which we start by creating a host builder for configuration
 */
var builder = Host.CreateDefaultBuilder(args);

// We then register the services to the IoC container, allowing us to inject them into other classes / services.
builder.ConfigureServices(services =>
{
    // Add database connection.
    var dbConnectionString = configuration.GetConnectionString("defaultConnection");
    services.AddDbContext<DatabaseContext>(options =>
    {
        options.UseLazyLoadingProxies();
        options.UseMySql(dbConnectionString, ServerVersion.AutoDetect(dbConnectionString));
    });

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