using System;
using Annium.Core.Application;
using Annium.Testing;

namespace Annium.Core.Application.Tests.Types
{
    public class TypeHelperTest
    {
        [Fact]
        public void ResolveProperty_Unary_Works()
        {
            // assert
            TypeHelper.ResolveProperty<A>(obj => obj.Demo).IsEqual(typeof(A).GetProperty(nameof(A.Demo)));
        }

        [Fact]
        public void ResolveProperty_Inner_Works()
        {
            // assert
            TypeHelper.ResolveProperty<B>(obj => obj.Inner.Demo).IsEqual(typeof(A).GetProperty(nameof(A.Demo)));
        }

        [Fact]
        public void ResolveProperty_NotProperty_Fails()
        {
            // assert
            ((Action) (() => TypeHelper.ResolveProperty<B>(obj => obj.Inner.ToString()))).Throws<ArgumentException>();
        }

        private class B
        {
            public A Inner { get; set; }
        }

        private class A
        {
            public string Demo { get; set; }
        }
    }
}