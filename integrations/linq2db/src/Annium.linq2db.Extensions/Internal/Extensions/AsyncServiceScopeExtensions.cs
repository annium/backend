using Microsoft.Extensions.DependencyInjection;

namespace Annium.linq2db.Extensions.Internal.Extensions;

/// <summary>
/// Extension helpers for cleaning up an <see cref="AsyncServiceScope"/> on a failure path.
/// </summary>
internal static class AsyncServiceScopeExtensions
{
    /// <summary>
    /// Disposes the scope without letting a cleanup failure mask the in-flight exception. A rollback
    /// error while disposing an already-broken connection must not replace the primary exception that
    /// triggered the cleanup, and there is no log sink available on this static path.
    /// </summary>
    /// <param name="scope">The scope to dispose.</param>
    public static void DisposeSafely(this AsyncServiceScope scope)
    {
        try
        {
            scope.Dispose();
        }
        catch
        {
            // intentionally suppressed — preserve the primary exception that triggered cleanup
        }
    }
}
