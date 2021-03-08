using System;

namespace Annium.Infrastructure.WebSockets.Domain.Responses
{
    public sealed class StreamEndResponse : ResponseBase
    {
        public StreamEndResponse(
            Guid rid
        ) : base(rid)
        {
        }
    }
}