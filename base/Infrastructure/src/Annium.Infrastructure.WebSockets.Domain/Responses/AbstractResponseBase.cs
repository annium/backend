using Annium.Core.Runtime.Types;

namespace Annium.Infrastructure.WebSockets.Domain.Responses
{
    public abstract class AbstractResponseBase
    {
        [ResolutionId]
        public string RType => GetType().GetIdString();
    }
}