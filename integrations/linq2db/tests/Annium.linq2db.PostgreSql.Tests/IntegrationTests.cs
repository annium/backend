using System.Threading.Tasks;
using Annium.linq2db.Tests.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.linq2db.PostgreSql.Tests;

public class IntegrationTests : IntegrationTestsBase
{
    public IntegrationTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        AddServicePack<ServicePack>();
    }

    [Fact]
    public async Task EndToEnd()
    {
        await EndToEnd_Base();
    }

    [Fact]
    public async Task HighLoad()
    {
        await HighLoad_Base();
    }
}