using System;
using System.Threading.Tasks;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Models
{
    internal interface IConnectionBoundStore
    {
        Task Cleanup(Guid connectionId);
    }
}