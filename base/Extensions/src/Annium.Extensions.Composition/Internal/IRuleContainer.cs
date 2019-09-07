using System.Threading.Tasks;

namespace Annium.Extensions.Composition
{
    internal interface IRuleContainer<TValue>
    {
        Task ComposeAsync(CompositionContext<TValue> context, TValue value);
    }
}