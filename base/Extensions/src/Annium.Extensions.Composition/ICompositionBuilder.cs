using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Composition
{
    public interface ICompositionBuilder<TValue, TField>
    {
        void LoadWith(Func<CompositionContext<TValue>, Task<TField>> load, string message = null, bool allowDefault = false);

        void LoadWith(Func<CompositionContext<TValue>, TField> load, string message = null, bool allowDefault = false);
    }
}