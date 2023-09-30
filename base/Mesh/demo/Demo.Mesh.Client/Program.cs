using Annium.Core.Entrypoint;
using Annium.Extensions.Arguments;
using Demo.Infrastructure.WebSockets.Client;
using Group = Demo.Infrastructure.WebSockets.Client.Commands.Group;

await using var entry = Entrypoint.Default
    .UseServicePack<ServicePack>()
    .Setup();

var (provider, ct) = entry;

Commander.Run<Group>(provider, args, ct);