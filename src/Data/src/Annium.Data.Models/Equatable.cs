using System;

namespace Annium.Data.Models
{
    public abstract class Equatable<T> : IEquatable<T> where T : Equatable<T>
    {
        public override abstract int GetHashCode();

        public bool Equals(T obj) => GetHashCode() == obj?.GetHashCode();

        public override bool Equals(object obj) => Equals(obj as T);

        public static bool operator ==(Equatable<T> a, Equatable<T> b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
                return true;

            // If one is null, but not both, return false.
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            // Return true if the fields match:
            return a.Equals(b);
        }

        public static bool operator !=(Equatable<T> a, Equatable<T> b) => !(a == b);
    }
}