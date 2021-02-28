using System;

namespace Annium.Infrastructure.WebSockets.Domain.Responses
{
    public abstract record ResponseBase : AbstractResponseBase
    {
        public Guid Rid { get; }

        public ResponseBase(Guid rid)
        {
            Rid = rid;
        }
    }
}