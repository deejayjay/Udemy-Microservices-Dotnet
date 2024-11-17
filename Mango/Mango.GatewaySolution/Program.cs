using Mango.GatewaySolution.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddAppAuthentication();

// Loads the Ocelot settings to builder
builder.Configuration.AddJsonFile("ocelot_settings.json", optional: false, reloadOnChange: true);

// Add Ocelot to the DI container
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

await app.UseOcelot();

app.Run();