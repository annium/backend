using Annium.AspNetCore.Extensions;
using Annium.AspNetCore.TestServer;
using Annium.Infrastructure.Hosting.Extensions;
using Annium.Logging.Microsoft;
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
    /// <summary>
    /// Entry point class for the AspNetCore test server
    /// </summary>
    public partial class Program;
}
