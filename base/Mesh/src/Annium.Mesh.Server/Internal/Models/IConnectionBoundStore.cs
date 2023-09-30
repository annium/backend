using System;
using System.Threading.Tasks;

namespace Annium.Mesh.Server.Internal.Models;

internal interface IConnectionBoundStore
{
    Task Cleanup(Guid connectionId);
}