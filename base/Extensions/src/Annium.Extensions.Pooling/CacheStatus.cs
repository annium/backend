namespace Annium.Extensions.Pooling
{
    internal enum CacheStatus
    {
        Creating,
        Active,
        Suspending,
        Suspended,
        Resuming
    }
}