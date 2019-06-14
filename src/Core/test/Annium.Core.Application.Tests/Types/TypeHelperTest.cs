using System;
using Annium.Core.Application.Types;
using Annium.Testing;

namespace Annium.Core.Application.Tests.Types
{
    public class TypeHelperTest
    {
        [Fact]
        public void ResolveProperty_Works()
        {
            // assert
            TypeHelper.ResolveProperty<A>(obj => obj.Demo).IsEqual(typeof(A).GetProperty(nameof(A.Demo)));
        }

        private class A
        {
            public string Demo { get; set; }
        }
    }
}