using System;
using Annium.Core.Application.Types;
using Annium.Testing;

namespace Annium.Core.Application.Tests.Types
{
    public class TypeExtensionsTest
    {
        [Fact]
        public void ResolveByImplentations_Null_ThrowsArgumentNullException()
        {
            ((Action) (() => (null as Type).ResolveByImplentations(typeof(int)))).Throws<ArgumentNullException>();
        }

        [Fact]
        public void ResolveByImplentations_Defined_IsReturned()
        {
            typeof(BasePlain).ResolveByImplentations(typeof(IPlain)).IsEqual(typeof(BasePlain));
        }

        [Fact]
        public void ResolveByImplentations_Interface_BuildsInterfaceImplementation()
        {
            typeof(ComplexPlain<>).ResolveByImplentations(typeof(IGeneric<int, int>)).IsEqual(typeof(ComplexPlain<int>));
        }

        [Fact]
        public void ResolveByImplentations_Parameter_ReturnsTargetIfMatchesRequirements()
        {
            typeof(ComplexPlain<>).GetGenericArguments() [0].ResolveByImplentations(typeof(IGeneric<int, int>)).IsEqual(typeof(IGeneric<int, int>));
        }

        [Fact]
        public void ResolveByImplentations_Class_BuildsClassImplementation()
        {
            typeof(OpenComplex<>).ResolveByImplentations(typeof(GenericPlain<bool>)).IsEqual(typeof(OpenComplex<bool>));
        }

        [Fact]
        public void ResolveByImplentations_Complex_BuildsClassImplementation()
        {
            typeof(MultiComplex<,>).ResolveByImplentations(typeof(OtherComplex<bool>), typeof(IGeneric<long, int>))
                .IsEqual(typeof(MultiComplex<bool, long>));
        }

        [Fact]
        public void ResolveByImplentations_InferableByConstraints_BuildsClassImplementation()
        {
            typeof(ConstrainedComplex<, , ,>).ResolveByImplentations(typeof(IOther<IGeneric<bool, IGeneric<bool, int>>>))
                .IsEqual(typeof(ConstrainedComplex<IGeneric<bool, IGeneric<bool, int>>, bool, IGeneric<bool, int>, int >));
        }

        [Fact]
        public void ResolveByImplentations_UnbuildableClass_ReturnsNull()
        {
            typeof(Other<>).ResolveByImplentations(typeof(Base<int, bool>)).IsEqual(typeof(Other<bool>));
            typeof(Other<>).ResolveByImplentations(typeof(Base<long, bool>)).IsDefault();
        }

        [Fact]
        public void ResolveByImplentations_UnbuildableInterface_ReturnsNull()
        {
            typeof(ComplexPlain<>).ResolveByImplentations(typeof(IGeneric<bool, int>)).IsEqual(typeof(ComplexPlain<bool>));
            typeof(ComplexPlain<>).ResolveByImplentations(typeof(IGeneric<int, bool>)).IsDefault();
        }

        [Fact]
        public void GetTargetImplementation_Null_ThrowsArgumentNullException()
        {
            ((Action) (() => (null as Type).GetTargetImplementation(typeof(int)))).Throws<ArgumentNullException>();
            ((Action) (() => typeof(int).GetTargetImplementation(null))).Throws<ArgumentNullException>();
        }

        [Fact]
        public void GetTargetImplementation_Assignable_IsReturned()
        {
            typeof(BasePlain).GetTargetImplementation(typeof(IPlain)).IsEqual(typeof(IPlain));
        }

        [Fact]
        public void GetTargetImplementation_SameGenericDefinition_ReturnsType()
        {
            var target = typeof(ValueTuple<,>);
            typeof(ValueTuple<int, bool>).GetTargetImplementation(target).IsEqual(typeof(ValueTuple<int, bool>));
        }

        [Fact]
        public void GetTargetImplementation_Interface_BuildsInterfaceImplementation()
        {
            var target = typeof(IGenericConstrained<,>).MakeGenericType(typeof(BasePlain), typeof(GenericPlain<>));
            typeof(OpenComplex<int>).GetTargetImplementation(target).IsEqual(typeof(IGenericConstrained<BasePlain, GenericPlain<int>>));
        }

        [Fact]
        public void GetTargetImplementation_Class_BuildsClassImplementation()
        {
            typeof(OpenComplex<int>).GetTargetImplementation(typeof(GenericPlain<>)).IsEqual(typeof(GenericPlain<int>));
        }

        [Fact]
        public void GetTargetImplementation_UnbuildableClass_ReturnsNull()
        {
            typeof(OpenComplex<int>).GetTargetImplementation(typeof(OtherComplex<>)).IsDefault();
        }

        [Fact]
        public void GetTargetImplementation_UnbuildableInterface_ReturnsNull()
        {
            var target = typeof(IGenericConstrained<,>).MakeGenericType(typeof(BasePlain), typeof(OpenComplex<>));
            typeof(OpenComplex<int>).GetTargetImplementation(target).IsDefault();
        }

        [Fact]
        public void GetOwnInterfaces_ReturnsOwnInterfaces()
        {
            // act
            var ownInterfaces = typeof(OpenComplex<int>).GetOwnInterfaces();

            // assert
            ownInterfaces.Has(1);
            ownInterfaces.At(0).IsEqual(typeof(IGenericConstrained<BasePlain, GenericPlain<int>>));
        }

        private class Other<T> : Next<int, T> { }

        private class Next<T1, T2> : Derived<T2, T1> { }

        private class Derived<T1, T2> : Base<T2, T1> { }

        private class Base<T1, T2> : IGeneric<T1, T2> { }

        private class OtherComplete : OtherComplex<bool>, IGeneric<bool, string> { }

        private class OtherComplex<T> : BasePlain { }

        private class OtherPlain { }

        private class ConstrainedComplex<T1, T2, T3, T4> : IOther<T1> where T1 : IGeneric<T2, T3> where T3 : IGeneric<T2, T4> { }

        private class MultiComplex<T1, T2> : OtherComplex<T1>, IGeneric<T2, int> { }

        private class OpenComplex<T> : ComplexPlain<T>, IGenericConstrained<BasePlain, GenericPlain<T>>, IPlain { }

        private class CompleteComplex : ComplexPlain<bool>, IGenericConstrained<BasePlain, GenericPlain<bool>> { }

        private class ComplexPlain<T> : GenericPlain<T>, IGeneric<T, int> { }

        private class GenericPlain<T> : BasePlain { }

        private class BasePlain : IPlain { }

        private interface IPlain { }

        private interface IGeneric<T1, T2> { }

        private interface IOther<T> { }

        private interface IGenericConstrained<T1, T2> where T2 : class, T1, new() where T1 : IPlain { }
    }
}