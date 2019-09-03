using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;

namespace Annium.Extensions.Composition
{
    public interface IComposer<TValue>
    {
        Task<IStatusResult<OperationStatus>> ComposeAsync(TValue value, string label = null);
    }
}