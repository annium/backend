using System;
using System.Collections.Generic;
using Annium.Core.Primitives;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperStructTests : TestBase
{
    [Theory]
    [InlineData(typeof(EmptyStruct))]
    [InlineData(typeof(IEmptyInterface))]
    [InlineData(typeof(EmptyRecord))]
    public void Empty(Type type)
    {
        // arrange
        var target = type.ToContextualType();

        // act
        var modelRef = Map(target).As<StructRef>();

        // assert
        modelRef.Namespace.Is(type.Namespace);
        modelRef.Name.Is(type.FriendlyName());
        modelRef.Args.IsEmpty();
        Models.Has(1);
        var model = Models.At(0).As<StructModel>();
        model.Namespace.Is(type.GetNamespace());
        model.Name.Is(type.FriendlyName());
        model.Args.IsEmpty();
        model.Base.IsDefault();
        model.Interfaces.IsEmpty();
        model.Fields.IsEmpty();
    }

    [Fact]
    public void Struct()
    {
        // arrange
        var type = typeof(Struct<string, HashSet<string>>);
        var target = type.ToContextualType();

        // act
        var modelRef = Map(target).As<StructRef>();

        // assert
        modelRef.Namespace.Is(type.Namespace);
        modelRef.Name.Is(nameof(Struct));
        modelRef.Args.Has(2);
        var refArg1 = modelRef.Args.At(0);
        refArg1
            .As<BaseTypeRef>().Name.Is(BaseType.String);
        var refArg2 = modelRef.Args.At(1);
        refArg2
            .As<ArrayRef>().Value
            .As<BaseTypeRef>().Name.Is(BaseType.String);

        Models.Has(3);
        var structModel = Models.At(0).As<StructModel>();
        structModel.Namespace.Is(typeof(Struct<,>).GetNamespace());
        structModel.Name.Is(nameof(Struct<object, object>));
        structModel.Args.Has(2);
        structModel.Args.At(0).Is(new GenericParameterRef("T1"));
        structModel.Args.At(1).Is(new GenericParameterRef("T2"));
        structModel.Base.IsDefault();
        structModel.Interfaces.Has(2);
        structModel.Interfaces.At(0).Is(new StructRef(
            typeof(IMulti<,>).GetNamespace().ToString(),
            nameof(IMulti<object, object>),
            new GenericParameterRef("T2"),
            new GenericParameterRef("T1")
        ));
        structModel.Interfaces.At(1).Is(new StructRef(
            typeof(Struct<,>).GetNamespace().ToString(),
            nameof(IUno<object>),
            new StructRef(
                typeof(Struct<,>).GetNamespace().ToString(),
                nameof(Struct<object, object>),
                new GenericParameterRef("T1"),
                new GenericParameterRef("T2")
            )
        ));
        structModel.Fields.Has(6);
        structModel.Fields.At(0).Is(new FieldModel(
            new BaseTypeRef(BaseType.String),
            nameof(Struct<int, int>.Name)
        ));
        structModel.Fields.At(1).Is(new FieldModel(
            new NullableRef(new BaseTypeRef(BaseType.String)),
            nameof(Struct<int, int>.Comment)
        ));
        structModel.Fields.At(2).Is(new FieldModel(
            new ArrayRef(new GenericParameterRef("T2")),
            nameof(Struct<int, int>.Data)
        ));
        structModel.Fields.At(3).Is(new FieldModel(
            new RecordRef(new BaseTypeRef(BaseType.String), new GenericParameterRef("T1")),
            nameof(Struct<int, int>.Values)
        ));
        structModel.Fields.At(4).Is(new FieldModel(
            new NullableRef(new BaseTypeRef(BaseType.Int)),
            nameof(Struct<int, int>.Ttl)
        ));
        structModel.Fields.At(5).Is(new FieldModel(
            new ArrayRef(
                new StructRef(
                    typeof(Struct<,>).GetNamespace().ToString(),
                    nameof(Struct<object, object>),
                    new GenericParameterRef("T1"),
                    new GenericParameterRef("T2")
                )
            ),
            nameof(Struct<int, int>.Items)
        ));

        var multi = Models.At(1).As<StructModel>();
        multi.Namespace.Is(typeof(IMulti<,>).GetNamespace());
        multi.Name.Is(nameof(IMulti<object, object>));
        multi.Args.Has(2);
        multi.Args.At(0).Is(new GenericParameterRef("T1"));
        multi.Args.At(1).Is(new GenericParameterRef("T2"));
        multi.Base.IsDefault();
        multi.Interfaces.IsEmpty();
        multi.Fields.Has(4);
        multi.Fields.At(0).Is(new FieldModel(
            new BaseTypeRef(BaseType.String),
            nameof(IMulti<int, int>.Name)
        ));
        multi.Fields.At(1).Is(new FieldModel(
            new NullableRef(new BaseTypeRef(BaseType.String)),
            nameof(IMulti<int, int>.Comment)
        ));
        multi.Fields.At(2).Is(new FieldModel(
            new ArrayRef(new GenericParameterRef("T1")),
            nameof(IMulti<int, int>.Data)
        ));
        multi.Fields.At(3).Is(new FieldModel(
            new RecordRef(new BaseTypeRef(BaseType.String), new GenericParameterRef("T2")),
            nameof(IMulti<int, int>.Values)
        ));

        var uno = Models.At(2).As<StructModel>();
        uno.Namespace.Is(typeof(IUno<>).GetNamespace());
        uno.Name.Is(nameof(IUno<int>));
        uno.Args.Has(1);
        uno.Args.At(0).Is(new GenericParameterRef("T"));
        uno.Base.IsDefault();
        uno.Interfaces.IsEmpty();
        uno.Fields.Has(2);
        uno.Fields.At(0).Is(new FieldModel(
            new NullableRef(new BaseTypeRef(BaseType.Int)),
            nameof(IUno<int>.Ttl)
        ));
        uno.Fields.At(1).Is(new FieldModel(
            new ArrayRef(
                new GenericParameterRef("T")
            ),
            nameof(IUno<int>.Items)
        ));
    }
}

file struct EmptyStruct
{
}

file interface IEmptyInterface
{
}

file record EmptyRecord;

file interface IMulti<T1, T2>
    where T1 : notnull
    where T2 : notnull
{
    string Name { get; }
    string? Comment { get; }
    T1[] Data { get; }
    IDictionary<string, T2?> Values { get; }
}

file interface IUno<T>
    where T : notnull
{
    int? Ttl { get; }
    List<T> Items { get; }
}

file record struct Struct<T1, T2>(
    string Name,
    string? Comment,
    T2[] Data,
    IDictionary<string, T1?> Values,
    int? Ttl,
    List<Struct<T1, T2>> Items
) : IMulti<T2, T1>, IUno<Struct<T1, T2>>
    where T1 : notnull
    where T2 : notnull;