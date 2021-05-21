using System;
using System.Threading.Tasks;

namespace Annium.Infrastructure.WebSockets.Client
{
    public interface ITestClient : IClientBase
    {
        event Func<Task> ConnectionLost;
    }
}