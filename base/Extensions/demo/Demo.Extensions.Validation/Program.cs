using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Extensions.Validation;
using Demo.Extensions.Validation;

await using var entry = Entrypoint.Default
    .UseServicePack<ServicePack>()
    .Setup();

var validator = entry.Provider.Resolve<IValidator<User>>();

var value = new User();
// ReSharper disable once UnusedVariable
var result = await validator.ValidateAsync(value);