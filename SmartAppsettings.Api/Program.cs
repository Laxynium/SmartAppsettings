using SmartAppsettings;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddReferencesReplacedSource();

var app = builder.Build();

app.MapGet("/", (IConfiguration configuration) => configuration.GetValue<string>("Auth:SigningKey"));

app.Run();