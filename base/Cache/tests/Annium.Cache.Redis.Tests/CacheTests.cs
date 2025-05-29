using System.Threading.Tasks;
using Annium.Cache.Tests.Lib;
using Xunit;

namespace Annium.Cache.Redis.Tests;

public class CacheTests : CacheTestsBase
{
    public CacheTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        AddServicePack<ServicePack>();
    }

    [Fact(Skip = "not implemented")]
    public async Task GetOrCreateAsync_Default()
    {
        await GetOrCreateAsync_Default_Base();
    }

    [Fact(Skip = "not implemented")]
    public async Task GetOrCreateAsync_AbsoluteExpiration()
    {
        await GetOrCreateAsync_AbsoluteExpiration_Base();
    }

    [Fact(Skip = "not implemented")]
    public async Task GetOrCreateAsync_SlidingExpiration()
    {
        await GetOrCreateAsync_SlidingExpiration_Base();
    }

    [Fact(Skip = "not implemented")]
    public async Task RemoveAsync()
    {
        await RemoveAsync_Base();
    }
}
