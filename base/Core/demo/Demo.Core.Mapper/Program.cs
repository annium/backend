using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Core.Mapper;
using Demo.Extensions.Mapping;

await using var entry = Entrypoint.Default
    .UseServicePack<ServicePack>()
    .Setup();

var mapper = entry.Provider.Resolve<IMapper>();

var value = new Plain { ClientName = "Den" };
_ = mapper.Map<Complex>(value);