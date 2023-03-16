using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Reflection.Tests.Types.Extensions;

public class ResolveGenericArgumentsByImplementationExtensionStructTests
{
    [Fact]
    public void OfNull_Throws()
    {
        // assert
        Wrap.It(() => (null as Type)!.ResolveGenericArgumentsByImplementation(typeof(bool)))
            .Throws<ArgumentNullException>();
        Wrap.It(() => typeof(bool).ResolveGenericArgumentsByImplementation(null!))
            .Throws<ArgumentNullException>();
    }

    [Fact]
    public void TypeNotGeneric_ReturnEmptyTypes()
    {
        // assert
        typeof(Array).ResolveGenericArgumentsByImplementation(typeof(IEnumerable))!.Is(Type.EmptyTypes);
    }

    [Fact]
    public void TypeDefined_ReturnsTypeArguments()
    {
        // assert
        typeof(List<int>).ResolveGenericArgumentsByImplementation(typeof(IEnumerable))!.IsEqual(
            new[] { typeof(int) });
    }

    [Fact]
    public void TargetNotGeneric_ReturnsTypeArguments()
    {
        // assert
        typeof(List<>).ResolveGenericArgumentsByImplementation(typeof(IEnumerable))!.IsEqual(new[]
            { typeof(List<>).GetGenericArguments()[0] });
    }

    [Fact]
    public void ParamOfParam_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IStructConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IClassConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void ParamOfParam_StructTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IClassConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IStructConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void ParamOfParam_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IClassConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(INewConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void ParamOfParam_ParameterConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IClassConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerableConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void ParamOfParam_ParameterConstraintSuccess_ReturnsType()
    {
        // assert
        typeof(IBase<,,,>).GetGenericArguments()[3]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerableConstraint<>).GetGenericArguments()[0])!
            .IsEqual(new[] { typeof(IBase<,,,>).GetGenericArguments()[3] });
    }

    [Fact]
    public void ParamOfClass_StructTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IStructConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(string))
            .IsDefault();
    }

    [Fact]
    public void ParamOfClass_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(INewConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(string))
            .IsDefault();
    }

    [Fact]
    public void ParamOfClass_ParameterConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IEnumerableConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(FileInfo))
            .IsDefault();
    }

    [Fact]
    public void ParamOfClass_ParameterConstraintSuccess_ReturnsType()
    {
        // assert
        typeof(IClassBaseConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(ClassBase))!
            .IsEqual(new[] { typeof(ClassBase) });
    }

    [Fact]
    public void ParamOfStruct_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IClassConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(long))
            .IsDefault();
    }

    [Fact]
    public void ParamOfStruct_StructTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IStructConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(long?))
            .IsDefault();
    }

    [Fact]
    public void
        ParamOfStruct_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(INewConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(ValueTuple<>))
            .IsDefault();
    }

    [Fact]
    public void ParamOfStruct_ParameterConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IEnumerableConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(bool))
            .IsDefault();
    }

    [Fact]
    public void ParamOfStruct_ParameterConstraintSuccess_ReturnsType()
    {
        // assert
        typeof(IEquatableConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(ValueTuple))!
            .IsEqual(new[] { typeof(ValueTuple) });
    }

    [Fact]
    public void ParamOfInterface_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IClassConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .IsDefault();
    }

    [Fact]
    public void ParamOfInterface_StructTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IStructConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .IsDefault();
    }

    [Fact]
    public void
        ParamOfInterface_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(INewConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .IsDefault();
    }

    [Fact]
    public void ParamOfInterface_ParameterConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IEquatableConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .IsDefault();
    }

    [Fact]
    public void ParamOfInterface_ParameterConstraintSuccess_ReturnsType()
    {
        // assert
        typeof(IEnumerableConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))!
            .IsEqual(new[] { typeof(IEnumerable) });
    }

    [Fact]
    public void ClassOfParam_StructTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(List<>)
            .ResolveGenericArgumentsByImplementation(typeof(IStructConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void ClassOfParam_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(ClassParametrized<>)
            .ResolveGenericArgumentsByImplementation(typeof(INewConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void ClassOfParam_ParameterConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(CustomDictionary<,>)
            .ResolveGenericArgumentsByImplementation(typeof(IClassBaseConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void ClassOfParam_ParameterConstraintSuccess_ReturnsType()
    {
        // assert
        typeof(ClassSimple)
            .ResolveGenericArgumentsByImplementation(typeof(IClassBaseConstraint<>).GetGenericArguments()[0])!
            .IsEqual(Type.EmptyTypes);
    }

    [Fact]
    public void ClassOfClass_SameGenericDefinition_BuildArgs()
    {
        // assert
        typeof(List<>).ResolveGenericArgumentsByImplementation(typeof(List<int>))!
            .IsEqual(new[] { typeof(int) });
    }

    [Fact]
    public void ClassOfClass_NullBaseType_ReturnsNull()
    {
        // assert
        typeof(HashSet<>).ResolveGenericArgumentsByImplementation(typeof(List<int>))
            .IsDefault();
    }

    [Fact]
    public void ClassOfClass_NotGenericBaseType_ReturnsNull()
    {
        // assert
        typeof(ClassParametrized<>).ResolveGenericArgumentsByImplementation(typeof(List<int>))
            .IsDefault();
    }

    [Fact]
    public void ClassOfClass_BaseTypeSameGenericDefinition_BuildArgs()
    {
        // assert
        typeof(CustomDictionary<,>).ResolveGenericArgumentsByImplementation(typeof(Dictionary<int, bool>))!
            .IsEqual(new[] { typeof(int), typeof(bool) });
    }

    [Fact]
    public void ClassOfClass_DifferentGenericDefinition_ResolvesBase()
    {
        // assert
        typeof(ParentDictionary<,>).ResolveGenericArgumentsByImplementation(typeof(Dictionary<int, bool>))!
            .IsEqual(new[] { typeof(bool), typeof(int) });
    }

    [Fact]
    public void ClassOfInterface_WithImplementation_BuildsArgs()
    {
        // assert
        typeof(Dictionary<,>).ResolveGenericArgumentsByImplementation(typeof(IReadOnlyDictionary<int, bool>))!
            .IsEqual(new[] { typeof(int), typeof(bool) });
    }

    [Fact]
    public void ClassOfInterface_NoImplementation_NoBaseType_ReturnsNull()
    {
        // assert
        typeof(List<>).ResolveGenericArgumentsByImplementation(typeof(IEquatable<int>))
            .IsDefault();
    }

    [Fact]
    public void ClassOfInterface_NoImplementation_WithBaseType_ResolvesBase()
    {
        // assert
        typeof(ParentDictionary<,>).ResolveGenericArgumentsByImplementation(typeof(IReadOnlyDictionary<int, bool>))!
            .IsEqual(new[] { typeof(bool), typeof(int) });
    }

    [Fact]
    public void StructOfParam_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(ValueTuple<>)
            .ResolveGenericArgumentsByImplementation(typeof(IClassConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void
        StructOfParam_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(ValueTuple<>)
            .ResolveGenericArgumentsByImplementation(typeof(INewConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void StructOfParam_ParameterConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(ValueTuple<>)
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerableConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void StructOfParam_ParameterConstraintSuccess_ReturnsType()
    {
        // assert
        typeof(StructEnumerable)
            .ResolveGenericArgumentsByImplementation(
                typeof(IEnumerableConstraint<>).GetGenericArguments()[0])!
            .IsEqual(Type.EmptyTypes);
    }

    [Fact]
    public void StructOfStruct_SameGenericDefinition_BuildArgs()
    {
        // assert
        typeof(ValueTuple<,>).ResolveGenericArgumentsByImplementation(typeof(ValueTuple<int, bool>))!
            .IsEqual(new[] { typeof(int), typeof(bool) });
    }

    [Fact]
    public void StructOfStruct_DifferentGenericDefinition_ReturnsNull()
    {
        // assert
        typeof(ValueTuple<,>).ResolveGenericArgumentsByImplementation(typeof(ValueTuple<int, string, bool>))
            .IsDefault();
    }

    [Fact]
    public void StructOfInterface_NoImplementation_ReturnsNull()
    {
        // assert
        typeof(ValueTuple<,>).ResolveGenericArgumentsByImplementation(typeof(IEquatable<bool>))
            .IsDefault();
    }

    [Fact]
    public void StructOfInterface_WithImplementation_BuildArgs()
    {
        // assert
        typeof(BaseStruct<,,,>).ResolveGenericArgumentsByImplementation(
                typeof(IBase<string, int, bool, IEnumerable<string>>))!
            .IsEqual(new[] { typeof(string), typeof(int), typeof(bool), typeof(IEnumerable<string>) });
    }

    [Fact]
    public void InterfaceOfParam_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplementation(typeof(IClassConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void InterfaceOfParam_StructTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplementation(typeof(IStructConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void
        InterfaceOfParam_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplementation(typeof(INewConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void InterfaceOfParam_ParameterConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplementation(typeof(IClassBaseConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void InterfaceOfParam_ParameterConstraintSuccess_ReturnsType()
    {
        // assert
        typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplementation(
                typeof(IEnumerableConstraint<>).GetGenericArguments()[0])!
            .IsEqual(new[] { typeof(IEnumerable<>).GetGenericArguments()[0] });
    }

    [Fact]
    public void InterfaceOfInterface_SameGenericDefinition_BuildsArgs()
    {
        // assert
        typeof(IEquatable<>).ResolveGenericArgumentsByImplementation(typeof(IEquatable<bool>))!
            .IsEqual(new[] { typeof(bool) });
    }

    [Fact]
    public void InterfaceOfInterface_NoImplementation_ReturnsNull()
    {
        // assert
        typeof(IEnumerable<>).ResolveGenericArgumentsByImplementation(typeof(IEquatable<bool>))
            .IsDefault();
    }

    [Fact]
    public void InterfaceOfInterface_WithImplementation_BuildArgs()
    {
        // assert
        typeof(IParentOther<,>).ResolveGenericArgumentsByImplementation(
                typeof(IBase<string[], int, bool, IEnumerable<string[]>>))!
            .IsEqual(new[] { typeof(string), typeof(int) });
    }

    [Fact]
    public void BuildArgs_InferByDefinitions()
    {
        // assert
        typeof(ConstrainedComplex<,,,>).ResolveGenericArgumentsByImplementation(
                typeof(IGeneric<IGeneric<bool, IGeneric<bool, int>>>))!
            .IsEqual(new[]
            {
                typeof(IGeneric<bool, IGeneric<bool, int>>), typeof(bool), typeof(IGeneric<bool, int>), typeof(int)
            });
    }

    private class ParentOther<T1, T2> : Base<T1[], T2, bool, IEnumerable<T1[]>>, IParentOther<T1, T2> where T2 : struct
    {
    }

    private class ParentTwo<T1, T2> : ParentOne<T1, IReadOnlyList<T2>>, IParentTwo<T1, T2> where T1 : struct where T2 : IEnumerable
    {
    }

    private class ParentOne<T1, T2> : Base<List<T2>, T1, int, IEnumerable<List<T2>>>, IParentOne<T1, T2> where T1 : struct
    {
    }

    private class Base<T1, T2, T3, T4> : IBase<T1, T2, T3, T4> where T1 : class where T2 : struct where T4 : IEnumerable<T1>
    {
    }

    private class ParentDictionary<T1, T2> : CustomDictionary<T2, T1> where T2 : notnull
    {
    }

    private class CustomDictionary<T1, T2> : Dictionary<T1, T2> where T1 : notnull
    {
    }

    private class ClassParametrized<T> : ClassBase
    {
        public T X { get; }

        public ClassParametrized(T x)
        {
            X = x;
        }
    }

    private class ClassSimple : ClassBase
    {
    }

    public struct BaseStruct<T1, T2, T3, T4> : IBase<T1, T2, T3, T4> where T1 : class where T2 : struct where T4 : IEnumerable<T1>
    {
    }

    private interface IParentOther<T1, T2> : IBase<T1[], T2, bool, IEnumerable<T1[]>> where T2 : struct
    {
    }

    private interface IParentTwo<T1, T2> : IParentOne<T1, IReadOnlyList<T2>> where T1 : struct where T2 : IEnumerable
    {
    }

    private interface IParentOne<T1, T2> : IBase<List<T2>, T1, int, IEnumerable<List<T2>>> where T1 : struct
    {
    }

    private interface IBase<T1, T2, T3, T4> where T1 : class where T2 : struct where T4 : IEnumerable<T1>
    {
    }

    public struct StructParamatered
    {
        public int X { get; }

        public StructParamatered(int x)
        {
            X = x;
        }
    }

    public struct StructParamaterless
    {
    }

    private struct StructEnumerable : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    private class ConstrainedComplex<T1, T2, T3, T4> : IGeneric<T1> where T1 : IGeneric<T2, T3> where T3 : IGeneric<T2, T4>
    {
    }

    private interface IGeneric<T>
    {
    }

    private interface IGeneric<T1, T2>
    {
    }

    private interface IClassConstraint<T> where T : class
    {
    }

    private interface IStructConstraint<T> where T : struct
    {
    }

    private interface INewConstraint<T> where T : new()
    {
    }

    private interface IClassBaseConstraint<T> where T : ClassBase
    {
    }

    private interface IEnumerableConstraint<T> where T : IEnumerable
    {
    }

    private interface IEquatableConstraint<T> where T : IEquatable<T>
    {
    }

    private class ClassBase
    {
    }
}