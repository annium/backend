using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;
using Annium.Core.Primitives;

namespace Annium.Core.DependencyInjection.Internal.Container
{
    internal class Index<TKey, TValue> : IIndex<TKey, TValue>
        where TKey : notnull
    {
        private readonly IEnumerable<KeyValue<TKey, TValue>> _keyValues;

        public Index(IEnumerable<KeyValue<TKey, TValue>> keyValues)
        {
            _keyValues = keyValues;
        }

        public TValue this[TKey key]
        {
            get
            {
                var values = _keyValues.Where(x => x.Key.Equals(key)).Select(x => x.Value).ToArray();

                return values.Length switch
                {
                    0 => throw new InvalidOperationException(
                        $"No {typeof(TValue).FriendlyName()} value registered for key {key} of type {typeof(TKey).FriendlyName()}"
                    ),
                    > 1 => throw new InvalidOperationException(
                        $"Ambiguous match of {values.Length} {typeof(TValue).FriendlyName()} values registered for key {key} of type {typeof(TKey).FriendlyName()}"
                    ),
                    _ => values[0]
                };
            }
        }

        public bool TryGetValue(TKey key, out TValue? value)
        {
            var values = _keyValues.Where(x => x.Key.Equals(key)).Select(x => x.Value).ToArray();
            value = default;

            switch (values.Length)
            {
                case 0:
                case > 1:
                    return false;
                default:
                    value = values[0];

                    return true;
            }
        }
    }
}