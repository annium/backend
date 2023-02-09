// ReSharper disable NotAccessedPositionalProperty.Local

namespace Annium.Net.Types.Tests.Mapper;

public class MapTests
{
    // [Fact]
    // public void Struct_SimpleStruct()
    // {
    //     // arrange
    //     var target = typeof(SimpleStruct);
    //
    //     // act
    //     var model = Map.ToModel(target).As<StructModel>();
    //
    //     // assert
    //     model.Namespace.Is(target.GetNamespace());
    //     model.Name.Is(target.FriendlyName());
    //     model.GenericArguments.IsEmpty();
    //     model.Base.IsDefault();
    //     model.Interfaces.IsEmpty();
    //     model.Fields.Has(1);
    //     model.Fields.At(0).Type.Is(Map.ToModel(typeof(int)));
    //     model.Fields.At(0).Name.Is(nameof(SimpleStruct.Value));
    //     model.IsGeneric.IsFalse();
    // }
    //
    // [Fact]
    // public void Struct_GenericStruct_Unbound()
    // {
    //     // arrange
    //     var target = typeof(GenericStruct<>);
    //
    //     // act
    //     var model = Map.ToModel(target).As<StructModel>();
    //
    //     // assert
    //     model.Namespace.Is(target.GetNamespace());
    //     model.Name.Is(target.FriendlyName());
    //     model.GenericArguments.Has(1);
    //     model.GenericArguments.At(0).Is(Map.ToModel(target.GetGenericArguments()[0]));
    //     model.Base.IsDefault();
    //     model.Interfaces.IsEmpty();
    //     model.Fields.Has(2);
    //     model.Fields.At(0).Type.Is(Map.ToModel(target.GetGenericArguments()[0]));
    //     model.Fields.At(0).Name.Is(nameof(GenericStruct<object>.Value));
    //     model.Fields.At(1).Type.Is(Map.ToModel(typeof(SimpleStruct)));
    //     model.Fields.At(1).Name.Is(nameof(GenericStruct<object>.Base));
    //     model.IsGeneric.IsTrue();
    // }
    //
    // [Fact]
    // public void Struct_GenericStruct_Bound()
    // {
    //     // arrange
    //     var target = typeof(GenericStruct<bool>);
    //
    //     // act
    //     var model = Map.ToModel(target).As<StructModel>();
    //
    //     // assert
    //     model.Namespace.Is(target.GetNamespace());
    //     model.Name.Is(target.FriendlyName());
    //     model.GenericArguments.Has(1);
    //     model.GenericArguments.At(0).Is(Map.ToModel(typeof(bool)));
    //     model.Base.IsDefault();
    //     model.Interfaces.IsEmpty();
    //     model.Fields.Has(2);
    //     model.Fields.At(0).Type.Is(Map.ToModel(typeof(bool)));
    //     model.Fields.At(0).Name.Is(nameof(GenericStruct<object>.Value));
    //     model.Fields.At(1).Type.Is(Map.ToModel(typeof(SimpleStruct)));
    //     model.Fields.At(1).Name.Is(nameof(GenericStruct<object>.Base));
    //     model.IsGeneric.IsFalse();
    // }
    //
    // [Fact]
    // public void Struct_SimpleInterface()
    // {
    //     // arrange
    //     var target = typeof(ISimpleInterface);
    //
    //     // act
    //     var model = Map.ToModel(target).As<StructModel>();
    //
    //     // assert
    //     model.Namespace.Is(target.GetNamespace());
    //     model.Name.Is(target.FriendlyName());
    //     model.GenericArguments.IsEmpty();
    //     model.Base.IsDefault();
    //     model.Interfaces.IsEmpty();
    //     model.Fields.Has(1);
    //     model.Fields.At(0).Type.Is(Map.ToModel(typeof(int)));
    //     model.Fields.At(0).Name.Is(nameof(ISimpleInterface.Value));
    //     model.IsGeneric.IsFalse();
    // }
    //
    // [Fact]
    // public void Struct_GenericInterface_Unbound()
    // {
    //     // arrange
    //     var target = typeof(IGenericInterface<>);
    //
    //     // act
    //     var model = Map.ToModel(target).As<StructModel>();
    //
    //     // assert
    //     model.Namespace.Is(target.GetNamespace());
    //     model.Name.Is(target.FriendlyName());
    //     model.GenericArguments.Has(1);
    //     model.GenericArguments.At(0).Is(Map.ToModel(target.GetGenericArguments()[0]));
    //     model.Base.IsDefault();
    //     model.Interfaces.IsEmpty();
    //     model.Fields.Has(2);
    //     model.Fields.At(0).Type.Is(Map.ToModel(target.GetGenericArguments()[0]));
    //     model.Fields.At(0).Name.Is(nameof(IGenericInterface<object>.Value));
    //     model.Fields.At(1).Type.Is(Map.ToModel(typeof(ISimpleInterface)));
    //     model.Fields.At(1).Name.Is(nameof(IGenericInterface<object>.Some));
    //     model.IsGeneric.IsTrue();
    // }
    //
    // [Fact]
    // public void Struct_GenericInterface_Bound()
    // {
    //     // arrange
    //     var target = typeof(IGenericInterface<bool>);
    //
    //     // act
    //     var model = Map.ToModel(target).As<StructModel>();
    //
    //     // assert
    //     model.Namespace.Is(target.GetNamespace());
    //     model.Name.Is(target.FriendlyName());
    //     model.GenericArguments.Has(1);
    //     model.GenericArguments.At(0).Is(Map.ToModel(typeof(bool)));
    //     model.Base.IsDefault();
    //     model.Interfaces.IsEmpty();
    //     model.Fields.Has(2);
    //     model.Fields.At(0).Type.Is(Map.ToModel(typeof(bool)));
    //     model.Fields.At(0).Name.Is(nameof(IGenericInterface<object>.Value));
    //     model.Fields.At(1).Type.Is(Map.ToModel(typeof(ISimpleInterface)));
    //     model.Fields.At(1).Name.Is(nameof(IGenericInterface<object>.Some));
    //     model.IsGeneric.IsFalse();
    // }
    //
    // [Fact]
    // public void Struct_SimpleRecord()
    // {
    //     // arrange
    //     var target = typeof(SimpleRecord);
    //
    //     // act
    //     var model = Map.ToModel(target).As<StructModel>();
    //
    //     // assert
    //     model.Namespace.Is(target.GetNamespace());
    //     model.Name.Is(target.FriendlyName());
    //     model.GenericArguments.IsEmpty();
    //     model.Base.IsDefault();
    //     model.Interfaces.IsEmpty();
    //     model.Fields.Has(1);
    //     model.Fields.At(0).Type.Is(Map.ToModel(typeof(int)));
    //     model.Fields.At(0).Name.Is(nameof(SimpleRecord.Value));
    //     model.IsGeneric.IsFalse();
    // }
    //
    // [Fact]
    // public void Struct_GenericRecord_Unbound()
    // {
    //     // arrange
    //     var target = typeof(GenericRecord<>);
    //
    //     // act
    //     var model = Map.ToModel(target).As<StructModel>();
    //
    //     // assert
    //     model.Namespace.Is(target.GetNamespace());
    //     model.Name.Is(target.FriendlyName());
    //     model.GenericArguments.Has(1);
    //     model.GenericArguments.At(0).Is(Map.ToModel(target.GetGenericArguments()[0]));
    //     model.Base.IsDefault();
    //     model.Interfaces.IsEmpty();
    //     model.Fields.Has(2);
    //     model.Fields.At(0).Type.Is(Map.ToModel(target.GetGenericArguments()[0]));
    //     model.Fields.At(0).Name.Is(nameof(GenericRecord<object>.Value));
    //     model.Fields.At(1).Type.Is(Map.ToModel(typeof(SimpleRecord)));
    //     model.Fields.At(1).Name.Is(nameof(GenericRecord<object>.Base));
    //     model.IsGeneric.IsTrue();
    // }
    //
    // [Fact]
    // public void Struct_GenericRecord_Bound()
    // {
    //     // arrange
    //     var target = typeof(GenericRecord<bool>);
    //
    //     // act
    //     var model = Map.ToModel(target).As<StructModel>();
    //
    //     // assert
    //     model.Namespace.Is(target.GetNamespace());
    //     model.Name.Is(target.FriendlyName());
    //     model.GenericArguments.Has(1);
    //     model.GenericArguments.At(0).Is(Map.ToModel(typeof(bool)));
    //     model.Base.IsDefault();
    //     model.Interfaces.IsEmpty();
    //     model.Fields.Has(2);
    //     model.Fields.At(0).Type.Is(Map.ToModel(typeof(bool)));
    //     model.Fields.At(0).Name.Is(nameof(GenericRecord<object>.Value));
    //     model.Fields.At(1).Type.Is(Map.ToModel(typeof(SimpleRecord)));
    //     model.Fields.At(1).Name.Is(nameof(GenericRecord<object>.Base));
    //     model.IsGeneric.IsFalse();
    // }
    //
    // [Fact]
    // public void Struct_InheritedRecord_Unbound()
    // {
    //     // arrange
    //     var target = typeof(InheritedRecord<,>);
    //     var t1 = target.GetGenericArguments()[0];
    //     var t2 = target.GetGenericArguments()[1];
    //
    //     // act
    //     var model = Map.ToModel(target).As<StructModel>();
    //
    //     // assert
    //     model.Namespace.Is(target.GetNamespace());
    //     model.Name.Is(target.FriendlyName());
    //     model.GenericArguments.Has(2);
    //     model.GenericArguments.At(0).Is(Map.ToModel(t1));
    //     model.GenericArguments.At(1).Is(Map.ToModel(t2));
    //     model.Base.Is(Map.ToModel(typeof(GenericRecord<>).MakeGenericType(t1)));
    //     model.Interfaces.Has(1);
    //     model.Interfaces.At(0).Is(Map.ToModel(typeof(IGenericInterface<>).MakeGenericType(t1)));
    //     model.Fields.Has(4);
    //     model.Fields.At(0).Type.Is(Map.ToModel(t2));
    //     model.Fields.At(0).Name.Is(nameof(InheritedRecord<object, ISimpleInterface>.Other));
    //     model.Fields.At(1).Type.Is(Map.ToModel(typeof(ISimpleInterface)));
    //     model.Fields.At(1).Name.Is(nameof(InheritedRecord<object, ISimpleInterface>.Some));
    //     model.Fields.At(2).Type.Is(Map.ToModel(t1));
    //     model.Fields.At(2).Name.Is(nameof(InheritedRecord<object, ISimpleInterface>.Value));
    //     model.Fields.At(3).Type.Is(Map.ToModel(typeof(SimpleRecord)));
    //     model.Fields.At(3).Name.Is(nameof(InheritedRecord<object, ISimpleInterface>.Base));
    //     model.IsGeneric.IsTrue();
    // }
    //
    // [Fact]
    // public void Struct_InheritedRecord_Bound()
    // {
    //     // arrange
    //     var target = typeof(InheritedRecord<bool, ISimpleInterface>);
    //     var t1 = target.GetGenericArguments()[0];
    //     var t2 = target.GetGenericArguments()[1];
    //
    //     // act
    //     var model = Map.ToModel(target).As<StructModel>();
    //
    //     // assert
    //     model.Namespace.Is(target.GetNamespace());
    //     model.Name.Is(target.FriendlyName());
    //     model.GenericArguments.Has(2);
    //     model.GenericArguments.At(0).Is(Map.ToModel(t1));
    //     model.GenericArguments.At(1).Is(Map.ToModel(t2));
    //     model.Base.Is(Map.ToModel(typeof(GenericRecord<>).MakeGenericType(t1)));
    //     model.Interfaces.Has(1);
    //     model.Interfaces.At(0).Is(Map.ToModel(typeof(IGenericInterface<>).MakeGenericType(t1)));
    //     model.Fields.Has(4);
    //     model.Fields.At(0).Type.Is(Map.ToModel(t2));
    //     model.Fields.At(0).Name.Is(nameof(InheritedRecord<object, ISimpleInterface>.Other));
    //     model.Fields.At(1).Type.Is(Map.ToModel(typeof(ISimpleInterface)));
    //     model.Fields.At(1).Name.Is(nameof(InheritedRecord<object, ISimpleInterface>.Some));
    //     model.Fields.At(2).Type.Is(Map.ToModel(t1));
    //     model.Fields.At(2).Name.Is(nameof(InheritedRecord<object, ISimpleInterface>.Value));
    //     model.Fields.At(3).Type.Is(Map.ToModel(typeof(SimpleRecord)));
    //     model.Fields.At(3).Name.Is(nameof(InheritedRecord<object, ISimpleInterface>.Base));
    //     model.IsGeneric.IsFalse();
    // }
}

file record RecordWithNullable(string? Value);

file enum SimpleEnum
{
    A = 1,
    B = 3
}

file record struct SimpleStruct(int Value);

file record struct GenericStruct<T>(T Value, SimpleStruct Base);

file interface ISimpleInterface
{
    int Value { get; }
}

file interface IGenericInterface<T>
{
    T Value { get; }
    ISimpleInterface Some { get; }
}

file record SimpleRecord(int Value);

file record GenericRecord<T>(T Value, SimpleRecord Base);

file record InheritedRecord<T1, T2>(T1 Value, SimpleRecord Base, T2 Other) : GenericRecord<T1>(Value, Base), IGenericInterface<T1>
    where T2 : ISimpleInterface
{
    public ISimpleInterface Some { get; } = Other;
}