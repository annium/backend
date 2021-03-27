using System.Threading;
using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Infrastructure.MessageBus.Node
{
    public interface IMessageBusClient
    {
        Task<IResult<T>> Fetch<T>(string topic, object request, CancellationToken ct = default);
    }
}
