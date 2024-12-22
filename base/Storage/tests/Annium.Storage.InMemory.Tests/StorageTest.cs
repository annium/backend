using Annium.Core.DependencyInjection;
using Annium.Storage.Abstractions;
using Annium.Storage.Base.Tests;

namespace Annium.Storage.InMemory.Tests;

public class StorageTest : StorageTestBase
{
    protected override IStorage GetStorage()
    {
        var services = new ServiceContainer();
        services.AddLogging();
        services.AddTime().WithManagedTime().SetDefault();
        services.AddInMemoryStorage("default", true);

        var provider = services.BuildServiceProvider();
        provider.UseLogging(x => x.UseInMemory());

        return provider.Resolve<IStorage>();
    }
}
