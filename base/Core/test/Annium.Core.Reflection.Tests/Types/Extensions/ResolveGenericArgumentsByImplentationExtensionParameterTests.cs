using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerableConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void Param_ParameterConstraintSuccess_ReturnsType()
    {
        //assert
        typeof(IListConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerableConstraint<>).GetGenericArguments()[0])!
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
        typeof(IEnumerableConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(FileInfo))
            .IsDefault();
    }

    [Fact]
    public void Class_ParameterConstraintSuccess_ReturnsType()
    {
        //assert
        typeof(IClassBaseConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(ClassBase))!
            .IsEqual(new[] { typeof(ClassBase) });
        typeof(IEnumerableConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(string))!
            .IsEqual(Type.EmptyTypes);
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
        typeof(IEnumerableConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(bool))
            .IsDefault();
    }

    [Fact]
    public void Struct_ParameterConstraintSuccess_ReturnsType()
    {
        //assert
        typeof(IEquatableConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(ValueTuple))
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
        typeof(IEquatableConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .IsDefault();
    }

    [Fact]
    public void Interface_ParameterConstraintSuccess_ReturnsType()
    {
        //assert
        typeof(IEnumerableConstraint<>).GetGenericArguments()[0]
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