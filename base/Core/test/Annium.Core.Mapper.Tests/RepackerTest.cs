using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests;

public class RepackerTest
{
    [Fact]
    public void Binary_Works()
    {
        // act
        var result = Repack<int?, int>(v => v ?? default);

        // assert
        result(1).IsEqual(1);
        result(null).IsEqual(0);
    }

    [Fact]
    public void Call_Works()
    {
        // act
        var result = Repack<int, string>(v => v.ToString());

        // assert
        result(1).IsEqual("1");
    }

    [Fact]
    public void Lambda_Works()
    {
        // act
        var result = Repack<int, int>(v => v);

        // assert
        result(1).IsEqual(1);
    }

    [Fact]
    public void Member_Works()
    {
        // act
        var result = Repack<string, int>(v => v.Length);

        // assert
        result("asd").IsEqual(3);
    }

    [Fact]
    public void MemberInit_Works()
    {
        // act
        var result = Repack<int, string>(v => new string(' ', v));

        // assert
        result(3).IsEqual("   ");
    }

    [Fact]
    public void Ternary_Works()
    {
        // act
        var result = Repack<string, string>(v =>
            v == "1" ? "one" :
            v == "2" ? "two" :
            "other"
        );

        // assert
        result("1").IsEqual("one");
        result("2").IsEqual("two");
        result("3").IsEqual("other");
    }

    [Fact]
    public void New_Works()
    {
        // act
        var result = Repack<string, Exception>(v => new Exception(v) { Source = v });

        // assert
        var ex = result("a");
        ex.Message.IsEqual("a");
        ex.Source.IsEqual("a");
    }

    [Fact]
    public void Unary_Works()
    {
        // act
        var result = Repack<bool, bool>(v => !v);

        // assert
        result(false).IsTrue();
    }

    [Fact]
    public void ListInit_Works()
    {
        // act
        var result = Repack<int, List<int>>(x => new List<int> { x });

        // assert
        result(5).Has(1).At(0).Is(5);
    }

    [Fact]
    public void NewArrayInit_Works()
    {
        // act
        var result = Repack<int, int[]>(x => new[] { x });

        // assert
        result(5).Has(1).At(0).Is(5);
    }

    private Func<TS, TR> Repack<TS, TR>(Expression<Func<TS, TR>> ex)
    {
        var repacker = new ServiceContainer()
            .AddRuntime(Assembly.GetCallingAssembly())
            .AddMapper(false)
            .BuildServiceProvider()
            .Resolve<IRepacker>();

        var param = Expression.Parameter(typeof(TS));

        return ((Expression<Func<TS, TR>>) repacker.Repack(ex)(param)).Compile();
    }
}