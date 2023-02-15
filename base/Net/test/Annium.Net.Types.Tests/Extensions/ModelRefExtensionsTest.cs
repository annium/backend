using System.Collections;
using System.Threading.Tasks;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Types.Tests.Extensions;

public class ModelRefExtensionsTest
{
    [Fact]
    public void IsFor_Struct()
    {
        var taskTRef =
            new StructRef(typeof(System.Threading.Tasks.Task<>).Namespace!, typeof(System.Threading.Tasks.Task<>).Name, new GenericParameterRef("T"));
        taskTRef.IsFor(typeof(System.Threading.Tasks.Task<>)).IsTrue();
        taskTRef.IsFor(typeof(Task<>)).IsFalse();
        taskTRef.IsFor(typeof(Task)).IsFalse();
    }

    [Fact]
    public void IsFor_Interface()
    {
        var taskTRef =
            new InterfaceRef(typeof(System.Collections.Generic.IEnumerable<>).Namespace!, typeof(System.Collections.Generic.IEnumerable<>).Name, new GenericParameterRef("T"));
        taskTRef.IsFor(typeof(System.Collections.Generic.IEnumerable<>)).IsTrue();
        taskTRef.IsFor(typeof(IEnumerable<>)).IsFalse();
        taskTRef.IsFor(typeof(IEnumerable)).IsFalse();
    }

    [Fact]
    public void IsFor_Enum()
    {
        var taskTRef = new EnumRef(typeof(System.UriKind).Namespace!, nameof(System.UriKind));
        taskTRef.IsFor(typeof(System.UriKind)).IsTrue();
        taskTRef.IsFor(typeof(UriKind)).IsFalse();
    }
}

file record struct Task<T>;

file interface IEnumerable<T>
{
}

file enum UriKind
{
}