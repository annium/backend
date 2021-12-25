using Annium.AspNetCore.TestServer;
using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseServicePack<ServicePack>();
builder.Logging.ConfigureLoggingBridge();

var app = builder.Build();

app.UseExceptionMiddleware();
app.UseWebSocketsServer();
app.UseRouting();
app.UseCorsDefaults();
app.UseEndpoints(endpoints => endpoints.MapControllers());

await app.RunAsync();

public partial class Program
{
}