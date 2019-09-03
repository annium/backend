using System.Threading.Tasks;
using Annium.Architecture.Base;

namespace Annium.Extensions.Composition
{
    internal interface ICompositionContainer<TValue>
    {
        Task ComposeAsync(CompositionContext<TValue> context, TValue value);
    }
}