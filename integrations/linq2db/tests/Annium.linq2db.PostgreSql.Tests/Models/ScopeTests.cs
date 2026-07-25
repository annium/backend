using System.Threading.Tasks;
using Annium.linq2db.Tests.Lib.Models;
using Xunit;

namespace Annium.linq2db.PostgreSql.Tests.Models;

/// <summary>
/// Tests for connection/transaction scope structs and their service-provider factory extensions
/// against the PostgreSQL backend.
/// </summary>
public class ScopeTests : ScopeTestsBase
{
    /// <summary>
    /// Initializes a new instance of the ScopeTests class
    /// </summary>
    /// <param name="outputHelper">Test output helper for logging</param>
    public ScopeTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        RegisterServicePack<ServicePack>();
    }

    /// <summary>
    /// Tests that a transaction scope rolls back on dispose using PostgreSQL backend
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task TransactionScope_RollsBackOnDispose()
    {
        await TransactionScope_RollsBackOnDispose_Base();
    }

    /// <summary>
    /// Tests that disposing a disposed-state transaction scope is a no-op using PostgreSQL backend
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task TransactionScope_DisposedState_DisposeIsNoop()
    {
        await TransactionScope_DisposedState_DisposeIsNoop_Base();
    }

    /// <summary>
    /// Tests that disposed-state transaction scopes guard their accessors
    /// </summary>
    [Fact]
    public void TransactionScope_DisposedState_Throws()
    {
        TransactionScope_DisposedState_Throws_Base();
    }

    /// <summary>
    /// Tests that disposed-state connection scopes guard their accessors
    /// </summary>
    [Fact]
    public void ConnectionScope_DisposedState_Throws()
    {
        ConnectionScope_DisposedState_Throws_Base();
    }

    /// <summary>
    /// Tests that disposing a disposed-state connection scope is a no-op using PostgreSQL backend
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task ConnectionScope_DisposedState_DisposeIsNoop()
    {
        await ConnectionScope_DisposedState_DisposeIsNoop_Base();
    }

    /// <summary>
    /// Tests that a connection scope from a disposed provider is itself disposed using PostgreSQL backend
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetConnectionScope_DisposedProvider_ReturnsDisposedScope()
    {
        await GetConnectionScope_DisposedProvider_ReturnsDisposedScope_Base();
    }

    /// <summary>
    /// Tests that a transaction scope from a disposed provider is itself disposed using PostgreSQL backend
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetTransactionScope_DisposedProvider_ReturnsDisposedScope()
    {
        await GetTransactionScope_DisposedProvider_ReturnsDisposedScope_Base();
    }

    /// <summary>
    /// Tests the multi-arity connection scope overloads against the PostgreSQL backend
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task ConnectionScope_MultiArity_Live()
    {
        await ConnectionScope_MultiArity_Live_Base();
    }

    /// <summary>
    /// Tests 2-arity transaction rollback on dispose against the PostgreSQL backend
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task TransactionScope_TwoArity_RollsBackOnDispose()
    {
        await TransactionScope_TwoArity_RollsBackOnDispose_Base();
    }

    /// <summary>
    /// Tests 3-arity transaction rollback on dispose against the PostgreSQL backend
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task TransactionScope_ThreeArity_RollsBackOnDispose()
    {
        await TransactionScope_ThreeArity_RollsBackOnDispose_Base();
    }

    /// <summary>
    /// Tests 4-arity transaction rollback on dispose against the PostgreSQL backend
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task TransactionScope_MultiArity_RollsBackOnDispose()
    {
        await TransactionScope_MultiArity_RollsBackOnDispose_Base();
    }
}
