using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Extensions.Composition;
using Demo.Extensions.Composition;

await using var entry = Entrypoint.Default
    .UseServicePack<ServicePack>()
    .Setup();

var composer = entry.Provider.Resolve<IComposer<User>>();

var value = new User();
var result = await composer.ComposeAsync(value);