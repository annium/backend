using System;
using Annium.Core.Runtime.Types;

namespace Annium.Infrastructure.WebSockets.Domain.Requests
{
    public abstract record AbstractRequestBase
    {
        public Guid Rid { get; init; } = Guid.NewGuid();

        [ResolutionId]
        public string RType => GetType().GetIdString();
    }
}