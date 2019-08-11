using System;
using Annium.Core.Application.Types;
using Annium.Testing;
namespace Annium.Core.Application.Tests.Types
{
    public class TypeExtensionsTest
    {
        [Fact]
        public void GetImplementationOf_Null_ThrowsArgumentNullException()
        {
            ((Action) (() => (null as Type).GetImplementationOf(typeof(int)))).Throws<ArgumentNullException>();
            ((Action) (() => typeof(int).GetImplementationOf(null))).Throws<ArgumentNullException>();
        }

        [Fact]
        public void GetImplementationOf_Assignable_IsReturned()
        {
            typeof(BasePlain).GetImplementationOf(typeof(IPlain)).IsEqual(typeof(IPlain));
        }

        [Fact]
        public void GetImplementationOf_Interface_BuildsInterfaceImplementation()
        {
            var target = typeof(IGenericConstrained<,>).MakeGenericType(typeof(BasePlain), typeof(GenericPlain<>));
            typeof(OpenComplex<int>).GetImplementationOf(target).IsEqual(typeof(IGenericConstrained<BasePlain, GenericPlain<int>>));
        }

        [Fact]
        public void GetImplementationOf_Class_BuildsClassImplementation()
        {
            typeof(OpenComplex<int>).GetImplementationOf(typeof(GenericPlain<>)).IsEqual(typeof(GenericPlain<int>));
        }

        [Fact]
        public void GetImplementationOf_UnbuildableClass_ReturnsNull()
        {
            typeof(OpenComplex<int>).GetImplementationOf(typeof(OtherComplex<>)).IsDefault();
        }

        [Fact]
        public void GetImplementationOf_UnbuildableInterface_ReturnsNull()
        {
            var target = typeof(IGenericConstrained<,>).MakeGenericType(typeof(BasePlain), typeof(OpenComplex<>));
            typeof(OpenComplex<int>).GetImplementationOf(target).IsDefault();
        }

        private class OtherComplete : OtherComplex<bool>, IGeneric<bool, string> { }

        private class OtherComplex<T> : BasePlain { }

        private class OtherPlain { }

        private class OpenComplex<T> : ComplexPlain<T>, IGenericConstrained<BasePlain, GenericPlain<T>> { }

        private class CompleteComplex : ComplexPlain<bool>, IGenericConstrained<BasePlain, GenericPlain<bool>> { }

        private class ComplexPlain<T> : GenericPlain<T>, IGeneric<T, int> { }

        private class GenericPlain<T> : BasePlain { }

        private class BasePlain : IPlain { }

        private interface IPlain { }

        private interface IGeneric<T1, T2> { }

        private interface IGenericConstrained<T1, T2> where T2 : class, T1, new() where T1 : IPlain { }
    }
}