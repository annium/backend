using System;
using Annium.Testing;

namespace Annium.Core.Reflection.Tests.Types.Extensions
{
    public class GetInheritanceChainExtensionTests
    {
        [Fact]
        public void GetInheritanceChain_OfNull_Throws()
        {
            //assert
            ((Action) (() => (null as Type) !.GetInheritanceChain())).Throws<ArgumentNullException>();
        }

        [Fact]
        public void GetInheritanceChain_Class_Root_Works()
        {
            //assert
            typeof(object).GetInheritanceChain().IsEmpty();
            typeof(object).GetInheritanceChain(self: true).Has(1).At(0).IsEqual(typeof(object));
            typeof(object).GetInheritanceChain(root: true).Has(1).At(0).IsEqual(typeof(object));
            typeof(object).GetInheritanceChain(self: true, root: true).Has(1).At(0).IsEqual(typeof(object));
        }

        [Fact]
        public void GetInheritanceChain_Class_NotInherited_Works()
        {
            //assert
            typeof(Sample).GetInheritanceChain().IsEmpty();
            typeof(Sample).GetInheritanceChain(self: true).Has(1).At(0).IsEqual(typeof(Sample));
            typeof(Sample).GetInheritanceChain(root: true).Has(1).At(0).IsEqual(typeof(object));
            typeof(Sample).GetInheritanceChain(self: true, root: true).Has(2).IsEqual(new [] { typeof(Sample), typeof(object) });
        }

        [Fact]
        public void GetInheritanceChain_Class_Inherited_Works()
        {
            //assert
            typeof(Derived).GetInheritanceChain().Has(1).At(0).IsEqual(typeof(Sample));
            typeof(Derived).GetInheritanceChain(self: true).Has(2).IsEqual(new [] { typeof(Derived), typeof(Sample) });
            typeof(Derived).GetInheritanceChain(root: true).Has(2).IsEqual(new [] { typeof(Sample), typeof(object) });
            typeof(Derived).GetInheritanceChain(self: true, root: true).Has(3).IsEqual(new [] { typeof(Derived), typeof(Sample), typeof(object) });
        }

        [Fact]
        public void GetInheritanceChain_Struct_Root_Works()
        {
            //assert
            typeof(ValueType).GetInheritanceChain().IsEmpty();
            typeof(ValueType).GetInheritanceChain(self: true).Has(1).At(0).IsEqual(typeof(ValueType));
            typeof(ValueType).GetInheritanceChain(root: true).Has(1).At(0).IsEqual(typeof(ValueType));
            typeof(ValueType).GetInheritanceChain(self: true, root: true).Has(1).At(0).IsEqual(typeof(ValueType));
        }

        [Fact]
        public void GetInheritanceChain_Struct_Works()
        {
            //assert
            typeof(Point).GetInheritanceChain().IsEmpty();
            typeof(Point).GetInheritanceChain(self: true).Has(1).At(0).IsEqual(typeof(Point));
            typeof(Point).GetInheritanceChain(root: true).Has(1).At(0).IsEqual(typeof(ValueType));
            typeof(Point).GetInheritanceChain(self: true, root: true).Has(2).IsEqual(new [] { typeof(Point), typeof(ValueType) });
        }

        [Fact]
        public void GetInheritanceChain_UnsupportedType_ReturnsEmptyChain()
        {
            //assert
            typeof(IFace).GetInheritanceChain().IsEmpty();
        }

        private class Derived : Sample { }

        private class Sample { }

        private struct Point { }

        private interface IFace { }
    }
}