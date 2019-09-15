using System;
using Annium.Testing;

namespace Annium.Core.Application.Tests.Types.Extensions
{
    public class GetOwnInterfacesExtensionTests
    {
        [Fact]
        public void GetOwnInterfaces_OfNull_Throws()
        {
            //assert
            ((Action) (() => (null as Type).GetOwnInterfaces())).Throws<ArgumentNullException>();
        }

        [Fact]
        public void GetOwnInterfaces_Works()
        {
            //assert
            typeof(Derived).GetOwnInterfaces().IsEqual(new [] { typeof(IDerived) });
            typeof(Base).GetOwnInterfaces().IsEqual(new [] { typeof(IBase), typeof(IInner), typeof(IShared) });
            typeof(IBase).GetOwnInterfaces().IsEqual(new [] { typeof(IInner) });
        }

        private class Derived : Base, IDerived, IShared { }
        private class Base : IBase, IShared { }
        private interface IDerived { }
        private interface IBase : IInner { }
        private interface IShared { }
        private interface IInner { }
    }
}