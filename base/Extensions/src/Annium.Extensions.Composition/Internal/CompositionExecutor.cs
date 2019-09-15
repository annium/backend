using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Reflection;
using Annium.Data.Operations;
using Annium.Localization.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.Composition
{
    internal class CompositionExecutor<TValue> : IComposer<TValue> where TValue : class
    {
        private static Type[] composerSets = typeof(TValue).GetInheritanceChain(self: true, root: false)
        .Concat(typeof(TValue).GetInterfaces())
        .Select(t => typeof(IEnumerable<>).MakeGenericType(typeof(ICompositionContainer<>).MakeGenericType(t)))
        .ToArray();

        private readonly ICompositionContainer<TValue>[] composers;

        private readonly ILocalizer<TValue> localizer;

        public CompositionExecutor(
            IServiceProvider serviceProvider
        )
        {
            composers = composerSets
                .Select(s => (IEnumerable<ICompositionContainer<TValue>>) serviceProvider.GetRequiredService(s))
                .SelectMany(v => v)
                .ToArray();

            var duplicates = GetDuplicates(composers);
            if (duplicates.Count > 0)
                throw new InvalidOperationException($@"{typeof(TValue)} has {duplicates.Count} properties with multiple loaders:{Environment.NewLine}{string.Join(Environment.NewLine, duplicates.Select(p => $"{p.Key.Name}: {string.Join(", ", p.Value)}"))}");

            localizer = serviceProvider.GetRequiredService<ILocalizer<TValue>>();
        }

        public async Task<IStatusResult<OperationStatus>> ComposeAsync(TValue value, string label = null)
        {
            var hasLabel = !string.IsNullOrWhiteSpace(label);

            if (value == null)
                return hasLabel ?
                    Result.Status(OperationStatus.BadRequest).Error(label, "Value is null") :
                    Result.Status(OperationStatus.BadRequest).Error("Value is null");

            if (composers.Length == 0)
                return Result.Status(OperationStatus.OK);

            var result = Result.New();

            foreach (var composer in composers)
                result.Join(await composer.ComposeAsync(value, label, localizer));

            return Result.Status(result.HasErrors ? OperationStatus.NotFound : OperationStatus.OK).Join(result);
        }

        private IReadOnlyDictionary<PropertyInfo, IList<Type>> GetDuplicates(ICompositionContainer<TValue>[] composers)
        {
            var duplicates = new Dictionary<PropertyInfo, IList<Type>>();
            foreach (var composer in composers)
                foreach (var field in composer.Fields)
                    if (duplicates.ContainsKey(field))
                        duplicates[field].Add(composer.GetType());
                    else
                        duplicates[field] = new List<Type> { composer.GetType() };

            return duplicates.Where(p => p.Value.Count > 1).ToDictionary(p => p.Key, p => p.Value);
        }
    }
}