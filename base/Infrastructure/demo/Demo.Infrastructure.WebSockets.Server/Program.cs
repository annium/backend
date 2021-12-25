using Annium.Core.DependencyInjection;
using Demo.Infrastructure.WebSockets.Server;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseServicePack<ServicePack>();
builder.Logging.ConfigureLoggingBridge();
builder.WebHost.UseKestrelDefaults();

var app = builder.Build();

app.UseWebSocketsServer();

await app.RunAsync();