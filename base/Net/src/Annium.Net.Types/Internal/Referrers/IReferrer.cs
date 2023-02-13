using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal interface IReferrer
{
    IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx);
}