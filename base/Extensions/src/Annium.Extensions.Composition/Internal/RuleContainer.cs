using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Annium.Extensions.Composition
{
    internal class RuleContainer<TValue, TField> : IRuleBuilder<TValue, TField>, IRuleContainer<TValue>
    {
        private readonly MethodInfo setTarget;
        private Delegate? load;
        private string message = string.Empty;
        private bool allowDefault;

        public RuleContainer(
            MethodInfo setTarget
        )
        {
            this.setTarget = setTarget;
        }

        public void LoadWith(
            Func<CompositionContext<TValue>, Task<TField>> load,
            string message = "",
            bool allowDefault = false
        )
        {
            this.load = load;
            this.message = message;
            this.allowDefault = allowDefault;
        }

        public void LoadWith(
            Func<CompositionContext<TValue>, TField> load,
            string message = "",
            bool allowDefault = false
        )
        {
            this.load = load;
            this.message = message;
            this.allowDefault = allowDefault;
        }

        public async Task ComposeAsync(CompositionContext<TValue> context, TValue value)
        {
            TField target = default !;

            switch (load)
            {
                case Func<CompositionContext<TValue>, Task<TField>> load:
                    target = await load(context);
                    break;
                case Func<CompositionContext<TValue>, TField> load:
                    target = load(context);
                    break;
            }

            if (target == null || (!allowDefault && target.Equals(default(TField) !)))
                context.Error(string.IsNullOrEmpty(message) ? "{0} not found" : message, context.Field);
            else
                setTarget.Invoke(value, new object[] { target });
        }
    }
}