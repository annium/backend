using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Application.Types;
using Annium.Data.Operations;
using Annium.Localization.Abstractions;

namespace Annium.Extensions.Composition
{
    public abstract class Composer<TValue> : IComposer<TValue>
    {
        private readonly IDictionary<PropertyInfo, ICompositionContainer<TValue>> containers =
        new Dictionary<PropertyInfo, ICompositionContainer<TValue>>();
        private readonly ILocalizer localizer;

        public Composer(
            ILocalizer localizer
        )
        {
            this.localizer = localizer;
        }

        protected ICompositionBuilder<TValue, TField> Field<TField>(
            Expression<Func<TValue, TField>> targetAccessor
        )
        {
            if (targetAccessor is null)
                throw new ArgumentNullException(nameof(targetAccessor));

            var target = TypeHelper.ResolveProperty(targetAccessor);
            var targetSetter = target.GetSetMethod(nonPublic: true) ??
                throw new ArgumentException("Target property has no setter", nameof(targetAccessor));

            var container = new CompositionContainer<TValue, TField>(targetSetter);

            containers[target] = container;

            return container;
        }

        public async Task<IStatusResult<OperationStatus>> ComposeAsync(TValue value, string label = null)
        {
            var hasLabel = !string.IsNullOrWhiteSpace(label);

            if (value == null)
                return hasLabel ?
                    Result.New(OperationStatus.BadRequest).Error(label, "Value is null") :
                    Result.New(OperationStatus.BadRequest).Error("Value is null");

            var result = Result.New();
            foreach (var(field, container) in containers)
            {
                var propertyLabel = hasLabel ? $"{label}.{field.Name}" : field.Name;
                var ruleResult = Result.New();
                var context = new CompositionContext<TValue>(value, propertyLabel, field.Name, ruleResult, localizer);

                await container.ComposeAsync(context, value);

                result.Join(ruleResult);
            };

            return Result.New(result.HasErrors ? OperationStatus.NotFound : OperationStatus.OK).Join(result);
        }
    }
}