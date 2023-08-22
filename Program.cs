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

/*
 * First we set up the IoC/DI system: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
 * We now begin setting up and configuring our application class, which we start by creating a generic host builder for configuration
 * https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host
 */
var builder = Host.CreateApplicationBuilder(args);

// We then register the services to the IoC container, allowing us to inject them into other classes / services.

/*
 * The CreateApplicationBuilder method:
 * Loads app configuration from:
 * 
 *  - appsettings.json.
 *  - appsettings.{Environment}.json.
 *  - Secret Manager when the app runs in the Development environment.
 *  - Environment variables.
 *  - Command-line arguments.
 *
 *  We can now get the desired connection string for the database out of one of those.
 */
var dbConnectionString = builder.Configuration.GetConnectionString("defaultConnection");

// Add our DB connection, this is scoped service that can be injected.
builder.Services.AddDbContext<DatabaseContext>(options =>
{
    options.UseLazyLoadingProxies();
    options.UseMySql(dbConnectionString, ServerVersion.AutoDetect(dbConnectionString));
});

// Register the repositories, they will be disposed when the service scope ends.
builder.Services.AddScoped<PlayerRepository>();
builder.Services.AddScoped<BannedWeaponRepository>();
builder.Services.AddScoped<GameServerRepository>();

// Game server listener. This is our hosted service.
builder.Services.AddHostedService<ListenerService>();

var app = builder.Build();

// Auto apply any pending db migrations on startup.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetService<DatabaseContext>();
    context.Database.Migrate();
}

app.Run();