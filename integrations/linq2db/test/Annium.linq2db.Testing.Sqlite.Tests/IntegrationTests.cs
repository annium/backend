using System.Threading.Tasks;
using Annium.linq2db.Tests.Lib;
using Xunit;

namespace Annium.linq2db.Testing.Sqlite.Tests;

public class IntegrationTests : IntegrationTestsBase
{
    public IntegrationTests()
    {
        AddServicePack<ServicePack>();
    }

    [Fact]
    public async Task EndToEnd()
    {
        await EndToEnd_Base();
    }
}