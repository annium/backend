using Annium.Core.Entrypoint;
using Annium.Mesh.Server.Web;
using Annium.Net.Servers.Web;
using Demo.Mesh.Server;

await using var entry = Entrypoint.Default
    .UseServicePack<ServicePack>()
    .Setup();

var (sp, ct) = entry;

var server = ServerBuilder.New(sp, 2727)
    .WithMeshHandler()
    .Build();

await server.RunAsync(ct);