using Annium.Core.Primitives;

namespace Annium.Core.Runtime.Internal.Time;

public interface ITimeProviderResolver
{
    ITimeProvider Resolve();
}