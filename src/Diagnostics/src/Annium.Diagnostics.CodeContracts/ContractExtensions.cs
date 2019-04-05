using System;

namespace Annium.Diagnostics.CodeContracts
{
    public static class ContractExtensions
    {
        public static T NotNull<T>(this T obj) where T : class
        {
            if (obj == null)
                throw new ArgumentNullException("Argument must not be null");

            return obj;
        }
    }
}