using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.Storage.Abstractions;
using Annium.Storage.Tests.Lib;
using Annium.Testing;
using Xunit;

namespace Annium.Storage.InMemory.Tests;

/// <summary>
/// Test class for in-memory storage implementation.
/// Inherits from StorageTestBase to run common storage tests against the in-memory provider.
/// </summary>
public class StorageTest : StorageTestBase
{
    /// <summary>
    /// Tests that when callers race to delete one item, exactly one of them is told it did the
    /// deleting — so a caller can act on having removed it without two callers both acting.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Delete_Concurrent_ReportsSuccessOnce()
    {
        // arrange: racers all released at once, to have them contend for the same item
        const int racers = 16;
        var storage = GetStorage();
        await storage.UploadAsync(new MemoryStream("sample text file"u8.ToArray()), "delete_concurrent");

        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var deletes = Enumerable
            .Range(0, racers)
            .Select(_ =>
                Task.Run(async () =>
                {
#pragma warning disable VSTHRD003 // intentionally awaiting the shared release barrier from within the racer
                    await gate.Task;
#pragma warning restore VSTHRD003

                    return await storage.DeleteAsync("delete_concurrent");
                })
            )
            .ToArray();

        // act
        gate.SetResult();
        var results = await Task.WhenAll(deletes);

        // assert
        results.Count(x => x).Is(1);
    }

    /// <summary>
    /// Creates and configures an in-memory storage instance for testing.
    /// Sets up dependency injection container with in-memory storage provider.
    /// </summary>
    /// <returns>A configured in-memory storage instance.</returns>
    protected override IStorage GetStorage() =>
        TestServices.BuildStorage(services => services.AddInMemoryStorage("default", true));
}
