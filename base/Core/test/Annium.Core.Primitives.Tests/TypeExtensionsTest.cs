using System.Collections;
using System.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Primitives.Tests
{
    public class TypeExtensionsTest
    {
        [Fact]
        public void FriendlyName_BaseType_Ok()
        {
            typeof(int).FriendlyName().IsEqual("int");
        }

        [Fact]
        public void FriendlyName_SimpleType_Ok()
        {
            typeof(IEnumerable).FriendlyName().IsEqual("IEnumerable");
        }

        [Fact]
        public void FriendlyName_GenericTypeDefinition_Ok()
        {
            typeof(IReadOnlyDictionary<,>).FriendlyName().IsEqual("IReadOnlyDictionary<TKey, TValue>");
        }

        [Fact]
        public void FriendlyName_GenericType_Ok()
        {
            typeof(IReadOnlyDictionary<string, IList<int?>>).FriendlyName().IsEqual("IReadOnlyDictionary<string, IList<int?>>");
        }
    }
}