using System;
using System.Collections.Generic;

namespace Annium.Data.Models
{
    public abstract class Equatable<T> : IEquatable<T> where T : Equatable<T>
    {
        public abstract override int GetHashCode();

        public bool Equals(T obj) => GetHashCode() == obj?.GetHashCode();

        public override bool Equals(object? obj) => Equals((T)obj!);

        public static bool operator ==(Equatable<T> a, Equatable<T> b)
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

        public static bool operator !=(Equatable<T> a, Equatable<T> b) => !(a == b);
    }
}