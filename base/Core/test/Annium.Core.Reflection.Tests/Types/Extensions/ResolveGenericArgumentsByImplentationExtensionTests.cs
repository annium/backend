using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Annium.Core.Reflection.Tests.Types.Extensions.ResolveGenericArgumentsByImplementation;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Reflection.Tests.Types.Extensions;

public class ResolveGenericArgumentsByImplementationExtensionTests
{
    [Fact]
    public void ResolveGenericArgumentsByImplementation_OfNull_Throws()
    {
        //assert
        ((Action) (() => (null as Type) !.ResolveGenericArgumentsByImplementation(typeof(bool))))
            .Throws<ArgumentNullException>();
        ((Action) (() => typeof(bool).ResolveGenericArgumentsByImplementation(null!)))
            .Throws<ArgumentNullException>();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_TypeNotGeneric_ReturnEmptyTypes()
    {
        //assert
        typeof(Array).ResolveGenericArgumentsByImplementation(typeof(IEnumerable)) !.IsEqual(Type.EmptyTypes);
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_TypeDefined_ReturnsTypeArguments()
    {
        //assert
        typeof(List<int>).ResolveGenericArgumentsByImplementation(typeof(IEnumerable)) !.IsEqual(
            new[] { typeof(int) });
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_TargetNotGeneric_ReturnsTypeArguments()
    {
        //assert
        typeof(List<>).ResolveGenericArgumentsByImplementation(typeof(IEnumerable)) !.IsEqual(new[]
            { typeof(List<>).GetGenericArguments()[0] });
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ParamOfParam_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IStructConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IClassConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ParamOfParam_StructTypeConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IClassConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IStructConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ParamOfParam_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IClassConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(INewConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ParamOfParam_ParameterConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IClassConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IParameterInterfaceConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ParamOfParam_ParameterConstraintSuccess_ReturnsType()
    {
        //assert
        typeof(IBase<,,,>).GetGenericArguments()[3]
            .ResolveGenericArgumentsByImplementation(
                typeof(IParameterInterfaceConstraint<>).GetGenericArguments()[0]) !
            .IsEqual(new[] { typeof(IBase<,,,>).GetGenericArguments()[3] });
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ParamOfClass_StructTypeConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IStructConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(string))
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ParamOfClass_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(INewConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(string))
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ParamOfClass_ParameterConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IParameterInterfaceConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(FileInfo))
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ParamOfClass_ParameterConstraintSuccess_ReturnsType()
    {
        //assert
        typeof(IParameterClassConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(ClassBase)) !
            .IsEqual(new[] { typeof(IParameterClassConstraint<>).GetGenericArguments()[0] });
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ParamOfStruct_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IClassConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(long))
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ParamOfStruct_StructTypeConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IStructConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(long?))
            .IsDefault();
    }

    [Fact]
    public void
        ResolveGenericArgumentsByImplementation_ParamOfStruct_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(INewConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(ValueTuple<>))
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ParamOfStruct_ParameterConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IParameterInterfaceConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(bool))
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ParamOfStruct_ParameterConstraintSuccess_ReturnsType()
    {
        //assert
        typeof(IParameterStructConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(ValueTuple)) !
            .IsEqual(new[] { typeof(IParameterStructConstraint<>).GetGenericArguments()[0] });
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ParamOfInterface_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IClassConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ParamOfInterface_StructTypeConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IStructConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .IsDefault();
    }

    [Fact]
    public void
        ResolveGenericArgumentsByImplementation_ParamOfInterface_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(INewConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ParamOfInterface_ParameterConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IParameterStructConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ParamOfInterface_ParameterConstraintSuccess_ReturnsType()
    {
        //assert
        typeof(IParameterInterfaceConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable)) !
            .IsEqual(new[] { typeof(IParameterInterfaceConstraint<>).GetGenericArguments()[0] });
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ClassOfParam_StructTypeConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(List<>)
            .ResolveGenericArgumentsByImplementation(typeof(IStructConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ClassOfParam_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(ClassParametrized<>)
            .ResolveGenericArgumentsByImplementation(typeof(INewConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ClassOfParam_ParameterConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(CustomDictionary<,>)
            .ResolveGenericArgumentsByImplementation(typeof(IParameterClassConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ClassOfParam_ParameterConstraintSuccess_ReturnsType()
    {
        //assert
        typeof(ClassSimple)
            .ResolveGenericArgumentsByImplementation(typeof(IParameterClassConstraint<>).GetGenericArguments()[0])!
            .IsEqual(Type.EmptyTypes);
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ClassOfClass_SameGenericDefinition_BuildArgs()
    {
        //assert
        typeof(List<>).ResolveGenericArgumentsByImplementation(typeof(List<int>)) !
            .IsEqual(new[] { typeof(int) });
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ClassOfClass_NullBaseType_ReturnsNull()
    {
        //assert
        typeof(HashSet<>).ResolveGenericArgumentsByImplementation(typeof(List<int>))
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ClassOfClass_NotGenericBaseType_ReturnsNull()
    {
        //assert
        typeof(ClassParametrized<>).ResolveGenericArgumentsByImplementation(typeof(List<int>))
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ClassOfClass_BaseTypeSameGenericDefinition_BuildArgs()
    {
        //assert
        typeof(CustomDictionary<,>).ResolveGenericArgumentsByImplementation(typeof(Dictionary<int, bool>)) !
            .IsEqual(new[] { typeof(int), typeof(bool) });
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ClassOfClass_DifferentGenericDefinition_ResolvesBase()
    {
        //assert
        typeof(ParentDictionary<,>).ResolveGenericArgumentsByImplementation(typeof(Dictionary<int, bool>)) !
            .IsEqual(new[] { typeof(bool), typeof(int) });
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ClassOfInterface_WithImplementation_BuildsArgs()
    {
        //assert
        typeof(Dictionary<,>).ResolveGenericArgumentsByImplementation(typeof(IReadOnlyDictionary<int, bool>)) !
            .IsEqual(new[] { typeof(int), typeof(bool) });
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ClassOfInterface_NoImplementation_NoBaseType_ReturnsNull()
    {
        //assert
        typeof(List<>).ResolveGenericArgumentsByImplementation(typeof(IEquatable<int>))
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_ClassOfInterface_NoImplementation_WithBaseType_ResolvesBase()
    {
        //assert
        typeof(ParentDictionary<,>).ResolveGenericArgumentsByImplementation(typeof(IReadOnlyDictionary<int, bool>)) !
            .IsEqual(new[] { typeof(bool), typeof(int) });
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_StructOfParam_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(ValueTuple<>)
            .ResolveGenericArgumentsByImplementation(typeof(IClassConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void
        ResolveGenericArgumentsByImplementation_StructOfParam_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(ValueTuple<>)
            .ResolveGenericArgumentsByImplementation(typeof(INewConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_StructOfParam_ParameterConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(ValueTuple<>)
            .ResolveGenericArgumentsByImplementation(typeof(IParameterInterfaceConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_StructOfParam_ParameterConstraintSuccess_ReturnsType()
    {
        //assert
        typeof(StructEnumerable)
            .ResolveGenericArgumentsByImplementation(
                typeof(IParameterInterfaceConstraint<>).GetGenericArguments()[0]) !
            .IsEqual(Type.EmptyTypes);
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_StructOfStruct_SameGenericDefinition_BuildArgs()
    {
        //assert
        typeof(ValueTuple<,>).ResolveGenericArgumentsByImplementation(typeof(ValueTuple<int, bool>)) !
            .IsEqual(new[] { typeof(int), typeof(bool) });
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_StructOfStruct_DifferentGenericDefinition_ReturnsNull()
    {
        //assert
        typeof(ValueTuple<,>).ResolveGenericArgumentsByImplementation(typeof(ValueTuple<int, string, bool>))
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_StructOfInterface_NoImplementation_ReturnsNull()
    {
        //assert
        typeof(ValueTuple<,>).ResolveGenericArgumentsByImplementation(typeof(IEquatable<bool>))
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_StructOfInterface_WithImplementation_BuildArgs()
    {
        //assert
        typeof(BaseStruct<,,,>).ResolveGenericArgumentsByImplementation(
                typeof(IBase<string, int, bool, IEnumerable<string>>)) !
            .IsEqual(new[] { typeof(string), typeof(int), typeof(bool), typeof(IEnumerable<string>) });
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_InterfaceOfParam_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplementation(typeof(IClassConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_InterfaceOfParam_StructTypeConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplementation(typeof(IStructConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void
        ResolveGenericArgumentsByImplementation_InterfaceOfParam_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplementation(typeof(INewConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_InterfaceOfParam_ParameterConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplementation(typeof(IParameterClassConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_InterfaceOfParam_ParameterConstraintSuccess_ReturnsType()
    {
        //assert
        typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplementation(
                typeof(IParameterInterfaceConstraint<>).GetGenericArguments()[0]) !
            .IsEqual(new[] { typeof(IEnumerable<>).GetGenericArguments()[0] });
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_InterfaceOfInterface_SameGenericDefinition_BuildsArgs()
    {
        //assert
        typeof(IEquatable<>).ResolveGenericArgumentsByImplementation(typeof(IEquatable<bool>)) !
            .IsEqual(new[] { typeof(bool) });
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_InterfaceOfInterface_NoImplementation_ReturnsNull()
    {
        //assert
        typeof(IEnumerable<>).ResolveGenericArgumentsByImplementation(typeof(IEquatable<bool>))
            .IsDefault();
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_InterfaceOfInterface_WithImplementation_BuildArgs()
    {
        //assert
        typeof(IParentOther<,>).ResolveGenericArgumentsByImplementation(
                typeof(IBase<string[], int, bool, IEnumerable<string[]>>)) !
            .IsEqual(new[] { typeof(string), typeof(int) });
    }

    [Fact]
    public void ResolveGenericArgumentsByImplementation_BuildArgs_InferByDefinitions()
    {
        //assert
        typeof(ConstrainedComplex<,,,>).ResolveGenericArgumentsByImplementation(
                typeof(IGeneric<IGeneric<bool, IGeneric<bool, int>>>)) !
            .IsEqual(new[]
            {
                typeof(IGeneric<bool, IGeneric<bool, int>>), typeof(bool), typeof(IGeneric<bool, int>), typeof(int)
            });
    }
}