using System;
using Annium.Core.Runtime.Types;

namespace Annium.Infrastructure.WebSockets.Domain.Requests
{
    public abstract class AbstractRequestBase
    {
        public Guid Rid { get; set; } = Guid.NewGuid();
        [ResolutionId] public string RType => GetType().GetIdString();
    }
}