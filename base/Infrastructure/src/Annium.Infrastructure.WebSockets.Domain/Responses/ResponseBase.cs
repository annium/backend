using System;

namespace Annium.Infrastructure.WebSockets.Domain.Responses
{
    public abstract class ResponseBase : AbstractResponseBase
    {
        public Guid Rid { get; }

        public ResponseBase(Guid rid)
        {
            Rid = rid;
        }
    }
}