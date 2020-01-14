using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using Annium.Core.Reflection.Tests.Types.Extensions.ResolveGenericArgumentsByImplentation;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Reflection.Tests.Types.Extensions
{
    public class ResolveGenericArgumentsByImplentationExtensionTests
    {
        [Fact]
        public void ResolveGenericArgumentsByImplentation_OfNull_Throws()
        {
            //assert
            ((Action) (() => (null as Type) !.ResolveGenericArgumentsByImplentation(typeof(bool)))).Throws<ArgumentNullException>();
            ((Action) (() => typeof(bool).ResolveGenericArgumentsByImplentation(null!))).Throws<ArgumentNullException>();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_TypeNotGeneric_ReturnEmptyTypes()
        {
            //assert
            typeof(Array).ResolveGenericArgumentsByImplentation(typeof(IEnumerable)) !.IsEqual(Type.EmptyTypes);
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_TypeDefined_ReturnsTypeArguments()
        {
            //assert
            typeof(List<int>).ResolveGenericArgumentsByImplentation(typeof(IEnumerable)) !.IsEqual(new [] { typeof(int) });
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_TargetNotGeneric_ReturnsTypeArguments()
        {
            //assert
            typeof(List<>).ResolveGenericArgumentsByImplentation(typeof(IEnumerable)) !.IsEqual(new [] { typeof(List<>).GetGenericArguments() [0] });
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ParamOfParam_ReferenceTypeConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(IStructConstraint<>).GetGenericArguments() [0]
                .ResolveGenericArgumentsByImplentation(typeof(IClassConstraint<>).GetGenericArguments() [0])
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ParamOfParam_StructTypeConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(IClassConstraint<>).GetGenericArguments() [0]
                .ResolveGenericArgumentsByImplentation(typeof(IStructConstraint<>).GetGenericArguments() [0])
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ParamOfParam_DefaultConstructorConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(IClassConstraint<>).GetGenericArguments() [0]
                .ResolveGenericArgumentsByImplentation(typeof(INewConstraint<>).GetGenericArguments() [0])
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ParamOfParam_ParameterConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(IClassConstraint<>).GetGenericArguments() [0]
                .ResolveGenericArgumentsByImplentation(typeof(IParameterInterfaceConstraint<>).GetGenericArguments() [0])
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ParamOfParam_ParameterConstraintSuccess_ReturnsType()
        {
            //assert
            typeof(IBase<, , ,>).GetGenericArguments() [3]
                .ResolveGenericArgumentsByImplentation(typeof(IParameterInterfaceConstraint<>).GetGenericArguments() [0]) !
                .IsEqual(new [] { typeof(IBase<, , ,>).GetGenericArguments() [3] });
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ParamOfClass_StructTypeConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(IStructConstraint<>).GetGenericArguments() [0]
                .ResolveGenericArgumentsByImplentation(typeof(string))
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ParamOfClass_DefaultConstructorConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(INewConstraint<>).GetGenericArguments() [0]
                .ResolveGenericArgumentsByImplentation(typeof(string))
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ParamOfClass_ParameterConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(IParameterInterfaceConstraint<>).GetGenericArguments() [0]
                .ResolveGenericArgumentsByImplentation(typeof(FileInfo))
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ParamOfClass_ParameterConstraintSuccess_ReturnsType()
        {
            //assert
            typeof(IParameterClassConstraint<>).GetGenericArguments() [0]
                .ResolveGenericArgumentsByImplentation(typeof(Expression)) !
                .IsEqual(new [] { typeof(IParameterClassConstraint<>).GetGenericArguments() [0] });
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ParamOfStruct_ReferenceTypeConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(IClassConstraint<>).GetGenericArguments() [0]
                .ResolveGenericArgumentsByImplentation(typeof(long))
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ParamOfStruct_StructTypeConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(IStructConstraint<>).GetGenericArguments() [0]
                .ResolveGenericArgumentsByImplentation(typeof(long?))
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ParamOfStruct_DefaultConstructorConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(INewConstraint<>).GetGenericArguments() [0]
                .ResolveGenericArgumentsByImplentation(typeof(ValueTuple<>))
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ParamOfStruct_ParameterConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(IParameterInterfaceConstraint<>).GetGenericArguments() [0]
                .ResolveGenericArgumentsByImplentation(typeof(bool))
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ParamOfStruct_ParameterConstraintSuccess_ReturnsType()
        {
            //assert
            typeof(IParameterStructConstraint<>).GetGenericArguments() [0]
                .ResolveGenericArgumentsByImplentation(typeof(ValueTuple)) !
                .IsEqual(new [] { typeof(IParameterStructConstraint<>).GetGenericArguments() [0] });
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ParamOfInterface_ReferenceTypeConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(IClassConstraint<>).GetGenericArguments() [0]
                .ResolveGenericArgumentsByImplentation(typeof(IEnumerable))
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ParamOfInterface_StructTypeConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(IStructConstraint<>).GetGenericArguments() [0]
                .ResolveGenericArgumentsByImplentation(typeof(IEnumerable))
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ParamOfInterface_DefaultConstructorConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(INewConstraint<>).GetGenericArguments() [0]
                .ResolveGenericArgumentsByImplentation(typeof(IEnumerable))
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ParamOfInterface_ParameterConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(IParameterStructConstraint<>).GetGenericArguments() [0]
                .ResolveGenericArgumentsByImplentation(typeof(IEnumerable))
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ParamOfInterface_ParameterConstraintSuccess_ReturnsType()
        {
            //assert
            typeof(IParameterInterfaceConstraint<>).GetGenericArguments() [0]
                .ResolveGenericArgumentsByImplentation(typeof(IEnumerable)) !
                .IsEqual(new [] { typeof(IParameterInterfaceConstraint<>).GetGenericArguments() [0] });
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ClassOfParam_StructTypeConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(List<>)
            .ResolveGenericArgumentsByImplentation(typeof(IStructConstraint<>).GetGenericArguments() [0])
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ClassOfParam_DefaultConstructorConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(ClassParametered<>)
            .ResolveGenericArgumentsByImplentation(typeof(INewConstraint<>).GetGenericArguments() [0])
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ClassOfParam_ParameterConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(ClassParametered<>)
            .ResolveGenericArgumentsByImplentation(typeof(IParameterClassConstraint<>).GetGenericArguments() [0])
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ClassOfParam_ParameterConstraintSuccess_ReturnsType()
        {
            //assert
            TypeManager.Instance.GetImplementations(typeof(MemberExpression)) [0]
                .ResolveGenericArgumentsByImplentation(typeof(IParameterClassConstraint<>).GetGenericArguments() [0]) !
                .IsEqual(Type.EmptyTypes);
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ClassOfClass_SameGenericDefinition_BuildArgs()
        {
            //assert
            typeof(List<>).ResolveGenericArgumentsByImplentation(typeof(List<int>)) !
                .IsEqual(new [] { typeof(int) });
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ClassOfClass_NullBaseType_ReturnsNull()
        {
            //assert
            typeof(HashSet<>).ResolveGenericArgumentsByImplentation(typeof(List<int>))
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ClassOfClass_NotGenericBaseType_ReturnsNull()
        {
            //assert
            typeof(ClassParametered<>).ResolveGenericArgumentsByImplentation(typeof(List<int>))
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ClassOfClass_BaseTypeSameGenericDefinition_BuildArgs()
        {
            //assert
            typeof(CustomDicitonary<,>).ResolveGenericArgumentsByImplentation(typeof(Dictionary<int, bool>)) !
                .IsEqual(new [] { typeof(int), typeof(bool) });
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ClassOfClass_DifferentGenericDefinition_ResolvesBase()
        {
            //assert
            typeof(ParentDictionary<,>).ResolveGenericArgumentsByImplentation(typeof(Dictionary<int, bool>)) !
                .IsEqual(new [] { typeof(bool), typeof(int) });
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ClassOfInterface_WithImplementation_BuildsArgs()
        {
            //assert
            typeof(Dictionary<,>).ResolveGenericArgumentsByImplentation(typeof(IReadOnlyDictionary<int, bool>)) !
                .IsEqual(new [] { typeof(int), typeof(bool) });
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ClassOfInterface_NoImplementation_NoBaseType_ReturnsNull()
        {
            //assert
            typeof(List<>).ResolveGenericArgumentsByImplentation(typeof(IEquatable<int>))
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_ClassOfInterface_NoImplementation_WithBaseType_ResolvesBase()
        {
            //assert
            typeof(ParentDictionary<,>).ResolveGenericArgumentsByImplentation(typeof(IReadOnlyDictionary<int, bool>)) !
                .IsEqual(new [] { typeof(bool), typeof(int) });
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_StructOfParam_ReferenceTypeConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(ValueTuple<>)
            .ResolveGenericArgumentsByImplentation(typeof(IClassConstraint<>).GetGenericArguments() [0])
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_StructOfParam_DefaultConstructorConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(ValueTuple<>)
            .ResolveGenericArgumentsByImplentation(typeof(INewConstraint<>).GetGenericArguments() [0])
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_StructOfParam_ParameterConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(ValueTuple<>)
            .ResolveGenericArgumentsByImplentation(typeof(IParameterInterfaceConstraint<>).GetGenericArguments() [0])
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_StructOfParam_ParameterConstraintSuccess_ReturnsType()
        {
            //assert
            typeof(StructEnumerable)
            .ResolveGenericArgumentsByImplentation(typeof(IParameterInterfaceConstraint<>).GetGenericArguments() [0]) !
                .IsEqual(Type.EmptyTypes);
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_StructOfStruct_SameGenericDefinition_BuildArgs()
        {
            //assert
            typeof(ValueTuple<,>).ResolveGenericArgumentsByImplentation(typeof(ValueTuple<int, bool>)) !
                .IsEqual(new [] { typeof(int), typeof(bool) });
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_StructOfStruct_DifferentGenericDefinition_ReturnsNull()
        {
            //assert
            typeof(ValueTuple<,>).ResolveGenericArgumentsByImplentation(typeof(ValueTuple<int, string, bool>))
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_StructOfInterface_NoImplementation_ReturnsNull()
        {
            //assert
            typeof(ValueTuple<,>).ResolveGenericArgumentsByImplentation(typeof(IEquatable<bool>))
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_StructOfInterface_WithImplementation_BuildArgs()
        {
            //assert
            typeof(BaseStruct<, , ,>).ResolveGenericArgumentsByImplentation(typeof(IBase<string, int, bool, IEnumerable<string>>)) !
                .IsEqual(new [] { typeof(string), typeof(int), typeof(bool), typeof(IEnumerable<string>) });
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_InterfaceOfParam_ReferenceTypeConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplentation(typeof(IClassConstraint<>).GetGenericArguments() [0])
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_InterfaceOfParam_StructTypeConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplentation(typeof(IStructConstraint<>).GetGenericArguments() [0])
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_InterfaceOfParam_DefaultConstructorConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplentation(typeof(INewConstraint<>).GetGenericArguments() [0])
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_InterfaceOfParam_ParameterConstraintFailure_ReturnsNull()
        {
            //assert
            typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplentation(typeof(IParameterClassConstraint<>).GetGenericArguments() [0])
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_InterfaceOfParam_ParameterConstraintSuccess_ReturnsType()
        {
            //assert
            typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplentation(typeof(IParameterInterfaceConstraint<>).GetGenericArguments() [0]) !
                .IsEqual(new [] { typeof(IEnumerable<>).GetGenericArguments() [0] });
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_InterfaceOfInterface_SameGenericDefinition_BuildsArgs()
        {
            //assert
            typeof(IEquatable<>).ResolveGenericArgumentsByImplentation(typeof(IEquatable<bool>)) !
                .IsEqual(new [] { typeof(bool) });
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_InterfaceOfInterface_NoImplementation_ReturnsNull()
        {
            //assert
            typeof(IEnumerable<>).ResolveGenericArgumentsByImplentation(typeof(IEquatable<bool>))
                .IsDefault();
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_InterfaceOfInterface_WithImplementation_BuildArgs()
        {
            //assert
            typeof(IParentOther<,>).ResolveGenericArgumentsByImplentation(typeof(IBase<string[], int, bool, IEnumerable<string[]>>)) !
                .IsEqual(new [] { typeof(string), typeof(int) });
        }

        [Fact]
        public void ResolveGenericArgumentsByImplentation_BuildArgs_InferByDefinitions()
        {
            //assert
            typeof(ConstrainedComplex<, , ,>).ResolveGenericArgumentsByImplentation(typeof(IGeneric<IGeneric<bool, IGeneric<bool, int>>>)) !
                .IsEqual(new [] { typeof(IGeneric<bool, IGeneric<bool, int>>), typeof(bool), typeof(IGeneric<bool, int>), typeof(int) });
        }
    }
}