using Annium.AspNetCore.DynamicControllers.TestServer;
using Annium.Infrastructure.Hosting;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseServicePack<ServicePack>();

var app = builder.Build();

app.UseRouting();
app.MapControllers();

await app.RunAsync();
