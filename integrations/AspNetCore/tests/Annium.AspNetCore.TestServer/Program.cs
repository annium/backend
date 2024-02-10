using Annium.AspNetCore.TestServer;
using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseServicePack<ServicePack>();
builder.Logging.ConfigureLoggingBridge();

var app = builder.Build();

app.UseExceptionMiddleware();
app.UseRouting();
app.UseCorsDefaults();
app.MapControllers();

await app.RunAsync();

namespace Annium.AspNetCore.TestServer
{
    public partial class Program;
}
