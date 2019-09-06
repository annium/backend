using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.Application.Types;
using Annium.Data.Operations;
using Annium.Localization.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.Validation
{
    internal class ValidationExecutor<TValue> : IValidator<TValue>
    {
        private static Type[] validatorSets = typeof(TValue).GetInheritanceChain(self: true, root: false)
        .Concat(typeof(TValue).GetInterfaces())
        .Select(t => typeof(IEnumerable<>).MakeGenericType(typeof(IValidationContainer<>).MakeGenericType(t)))
        .ToArray();

        private readonly IValidationContainer<TValue>[] validators;

        private readonly ILocalizer<TValue> localizer;

        public ValidationExecutor(
            IServiceProvider serviceProvider
        )
        {
            validators = validatorSets
                .Select(s => (IEnumerable<IValidationContainer<TValue>>) serviceProvider.GetRequiredService(s))
                .SelectMany(v => v)
                .ToArray();

            localizer = serviceProvider.GetRequiredService<ILocalizer<TValue>>();
        }

        public async Task<IResult> ValidateAsync(TValue value, string label = null)
        {
            var hasLabel = !string.IsNullOrWhiteSpace(label);

            if (value == null)
                return hasLabel ?
                    Result.New().Error(label, "Value is null") :
                    Result.New().Error("Value is null");

            if (validators.Length == 0)
                return Result.New();

            var result = Result.New();
            var stage = 0;
            var ranStage = false;
            do
            {
                ranStage = false;

                foreach (var validator in validators)
                {
                    var(validatorResult, hasRun) = await validator.ValidateAsync(value, label, stage, localizer);

                    result.Join(validatorResult);
                    ranStage = hasRun || ranStage;
                }

                // short-circuit if any errors after stage execution
                if (result.HasErrors)
                    return result;

                // go next stage, if there was any run on current
                stage++;
            } while (ranStage);

            return result;
        }
    }
}