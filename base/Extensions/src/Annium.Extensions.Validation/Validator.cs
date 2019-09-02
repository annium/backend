using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Core.Application.Types;
using Annium.Data.Operations;
using Annium.Localization.Abstractions;

namespace Annium.Extensions.Validation
{
    public abstract class Validator<TValue> : IValidator<TValue>
    {
        private readonly IDictionary<PropertyInfo, IRuleContainer<TValue>> rules =
        new Dictionary<PropertyInfo, IRuleContainer<TValue>>();
        private readonly ILocalizer localizer;

        public Validator(
            ILocalizer localizer
        )
        {
            this.localizer = localizer;
        }

        protected IRuleBuilder<TValue, TField> Field<TField>(Expression<Func<TValue, TField>> accessor)
        {
            if (accessor is null)
                throw new ArgumentNullException(nameof(accessor));

            var property = TypeHelper.ResolveProperty(accessor);
            var rule = new RuleContainer<TValue, TField>(accessor.Compile());

            rules[property] = rule;

            return rule;
        }

        public async Task<IResult> ValidateAsync(TValue value, string label = null)
        {
            var hasLabel = !string.IsNullOrWhiteSpace(label);

            if (value == null)
                return hasLabel ?
                    Result.New().Error(label, "Value is null") :
                    Result.New().Error("Value is null");

            if (rules.Count == 0)
                return Result.New();

            var result = Result.New();
            var stageCount = rules.Max(r => r.Value.StageCount);
            for (var stage = 0; stage < stageCount; stage++)
            {
                foreach (var(property, rule) in rules)
                {
                    var propertyLabel = hasLabel ? $"{label}.{property.Name}" : property.Name;
                    var ruleResult = Result.New();
                    var context = new ValidationContext<TValue>(value, propertyLabel, property.Name, ruleResult, localizer);
                    await rule.ValidateAsync(context, value, stage);
                    result.Join(ruleResult);
                }

                // short-circuit if any errors after stage execution
                if (result.HasErrors)
                    return result;
            }

            return result;
        }
    }
}