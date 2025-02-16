using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Storage.Abstractions;
using Annium.Storage.Base.Tests;

namespace Annium.Storage.S3.Tests;

internal class StorageTest : StorageTestBase, IAsyncDisposable
{
    protected override IStorage GetStorage()
    {
        var services = new ServiceContainer();
        services.AddLogging();
        services.AddTime().WithManagedTime().SetDefault();
        var config = new Configuration
        {
            Server = "https://s3.yandexcloud.net",
            AccessKey = "AccessKey",
            AccessSecret = "AccessSecret",
            Region = "us-east-1",
            Bucket = "annium.tests",
            Directory = "/files",
        };
        services.AddS3Storage("default", (_, _) => config, true);

        var provider = services.BuildServiceProvider();
        provider.UseLogging(x => x.UseInMemory());

        return provider.Resolve<IStorage>();
    }

    public async ValueTask DisposeAsync()
    {
        var storage = GetStorage();
        foreach (var item in await storage.ListAsync())
            await storage.DeleteAsync(item);
    }
}
