using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Annium.Core.Application.Types;
using Annium.Data.Operations;

namespace Annium.Extensions.Validation
{
    public abstract class Validator<TValue>
    {
        private IDictionary<PropertyInfo, IRuleContainer<TValue>> rules = new Dictionary<PropertyInfo, IRuleContainer<TValue>>();

        protected IRule<TField> Field<TField>(Expression<Func<TValue, TField>> accessor)
        {
            if (accessor is null)
                throw new ArgumentNullException(nameof(accessor));

            var property = TypeHelper.ResolveProperty(accessor);
            var rule = new Rule<TValue, TField>(accessor.Compile());

            rules[property] = rule;

            return rule;
        }

        public BooleanResult Validate(TValue value, string label = null)
        {
            if (value == null)
                return string.IsNullOrWhiteSpace(label) ?
                    Result.Failure().Error("Value is null") :
                    Result.Failure().Error(label, "Value is null");

            var results = new List<IResult>();

            foreach (var(property, rule) in rules)
            {
                var propertyLabel = string.IsNullOrWhiteSpace(label) ? property.Name : $"{label}.{property.Name}";
                var result = rule.Validate(value, propertyLabel);
                results.Add(result);
            };

            return Result.Success().Join(results);
        }
    }
}