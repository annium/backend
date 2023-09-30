using System;
using System.Threading.Tasks;

namespace Annium.Mesh.Client;

public interface ITestClient : IClientBase
{
    event Func<Task> ConnectionLost;
}