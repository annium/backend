using System.Threading.Tasks;
using Annium.Cache.Tests.Lib;
using Xunit;

namespace Annium.Cache.Redis.Tests;

public class CacheTests : CacheTestsBase
{
    public CacheTests()
    {
        AddServicePack<ServicePack>();
    }

    [Fact]
    public async Task GetOrCreateAsync_Default()
    {
        await GetOrCreateAsync_Default_Base();
    }

    [Fact]
    public async Task GetOrCreateAsync_AbsoluteExpiration()
    {
        await GetOrCreateAsync_AbsoluteExpiration_Base();
    }

    [Fact]
    public async Task GetOrCreateAsync_SlidingExpiration()
    {
        await GetOrCreateAsync_SlidingExpiration_Base();
    }

    [Fact]
    public async Task RemoveAsync()
    {
        await RemoveAsync_Base();
    }
}