using System;

namespace Annium.Infrastructure.WebSockets.Domain.Responses
{
    public sealed record StreamEndResponse : ResponseBase
    {
        public StreamEndResponse(
            Guid rid
        ) : base(rid)
        {
        }
    }
}