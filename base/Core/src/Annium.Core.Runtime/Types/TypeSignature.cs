using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Core.Runtime.Types
{
    public sealed class TypeSignature : IEquatable<TypeSignature>
    {
        public static TypeSignature Create(object instance) => Create(instance.GetType() ?? throw new ArgumentNullException(nameof(instance)));
        public static TypeSignature Create(Type type) => Create(type.GetProperties().Select(x => x.Name));

        public static TypeSignature Create(params string[] signature) => Create(signature.AsEnumerable());

        public static TypeSignature Create(IEnumerable<string> signature)
        {
            var normalizedSignature = signature.Select(x => x.ToLowerInvariant()).Distinct().OrderBy(x => x).ToArray();

            return new TypeSignature(normalizedSignature);
        }

        public int Size => _signature.Count;

        private readonly IReadOnlyCollection<string> _signature;

        private TypeSignature(IReadOnlyCollection<string> signature)
        {
            if (signature is null)
                throw new ArgumentNullException(nameof(signature));

            _signature = signature;
        }

        public int GetMatchTo(TypeSignature target)
        {
            // if target signature contains any different items - no match
            if (target._signature.Any(x => !_signature.Contains(x)))
                return 0;

            // return number of items of target signature, this signature contains
            return target._signature.Count(_signature.Contains);
        }

        public bool Equals(TypeSignature obj) => GetHashCode() == obj?.GetHashCode();

        public override bool Equals(object? obj) => Equals((TypeSignature) obj!);

        public override int GetHashCode() => HashCode.Combine(_signature);

        public static bool operator ==(TypeSignature a, TypeSignature b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
                return true;

            // If one is null, but not both, return false.
            if (a is null || b is null)
                return false;

            // Return true if the fields match:
            return a.Equals(b);
        }

        public static bool operator !=(TypeSignature a, TypeSignature b) => !(a == b);
    }
}