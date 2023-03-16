using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Annium.Core.Reflection.Tests.Types.Extensions.ResolveGenericArgumentsByImplementation;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Reflection.Tests.Types.Extensions;

public class ResolveGenericArgumentsByImplementationExtensionParameterTests
{
    [Fact]
    public void Param_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IStructConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IClassConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void Param_StructTypeConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IClassConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IStructConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void Param_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IClassConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(INewConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void Param_ParameterConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IClassConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IParameterInterfaceConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void Param_ParameterConstraintSuccess_ReturnsType()
    {
        //assert
        typeof(IListConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IParameterInterfaceConstraint<>).GetGenericArguments()[0])!
            .IsEqual(new[] { typeof(IListConstraint<>).GetGenericArguments()[0] });
    }

    [Fact]
    public void Class_StructTypeConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IStructConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(string))
            .IsDefault();
    }

    [Fact]
    public void Class_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(INewConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(string))
            .IsDefault();
    }

    [Fact]
    public void Class_ParameterConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IParameterInterfaceConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(FileInfo))
            .IsDefault();
    }

    [Fact]
    public void Class_ParameterConstraintSuccess_ReturnsType()
    {
        //assert
        typeof(IParameterClassConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(ClassBase))!
            .IsEqual(new[] { typeof(ClassBase) });
    }

    [Fact]
    public void Struct_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IClassConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(long))
            .IsDefault();
    }

    [Fact]
    public void Struct_StructTypeConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IStructConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(long?))
            .IsDefault();
    }

    [Fact]
    public void Struct_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(INewConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(ValueTuple<>))
            .IsDefault();
    }

    [Fact]
    public void Struct_ParameterConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IParameterInterfaceConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(bool))
            .IsDefault();
    }

    [Fact]
    public void Struct_ParameterConstraintSuccess_ReturnsType()
    {
        //assert
        typeof(IParameterCuriouslyRecurringConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(ValueTuple))!
            .IsEqual(new[] { typeof(ValueTuple) });
    }

    [Fact]
    public void Interface_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IClassConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .IsDefault();
    }

    [Fact]
    public void Interface_StructTypeConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IStructConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .IsDefault();
    }

    [Fact]
    public void
        Interface_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(INewConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .IsDefault();
    }

    [Fact]
    public void Interface_ParameterConstraintFailure_ReturnsNull()
    {
        //assert
        typeof(IParameterCuriouslyRecurringConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .IsDefault();
    }

    [Fact]
    public void Interface_ParameterConstraintSuccess_ReturnsType()
    {
        //assert
        typeof(IParameterInterfaceConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))!
            .IsEqual(new[] { typeof(IEnumerable) });
    }

    private interface IListConstraint<T> where T : List<T>
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

    private interface IParameterClassConstraint<T> where T : ClassBase
    {
    }

    private interface IParameterInterfaceConstraint<T> where T : IEnumerable
    {
    }

    private interface IParameterCuriouslyRecurringConstraint<T> where T : IEquatable<T>
    {
    }
}