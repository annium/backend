using System;
using System.IO;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Storage.Abstractions;
using Annium.Storage.Base.Tests;

namespace Annium.Storage.FileSystem.Tests;

public class StorageTest : StorageTestBase, IAsyncDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath().TrimEnd('/'), Guid.NewGuid().ToString());

    protected override IStorage GetStorage()
    {
        var services = new ServiceContainer();
        services.AddLogging();
        services.AddTime().WithManagedTime().SetDefault();
        Directory.CreateDirectory(_directory);
        services.AddFileSystemStorage("default", (_, _) => new Configuration { Directory = _directory }, true);

        var provider = services.BuildServiceProvider();
        provider.UseLogging(x => x.UseInMemory());

        return provider.Resolve<IStorage>();
    }

    public ValueTask DisposeAsync()
    {
        Directory.Delete(_directory, true);
        return ValueTask.CompletedTask;
    }
}
