using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Annium.Core.Entrypoint;
using Annium.Core.Reflection;
using Annium.Core.Reflection.Tests.Types.Extensions.ResolveGenericArgumentsByImplementation;
using Annium.Core.Runtime.Types;

namespace Demo.Core.Reflection;

public class Program
{
    private static void Run(
        IServiceProvider provider,
        string[] args,
        CancellationToken ct
    )
    {
        var typeManager = TypeManager.GetInstance(typeof(Program).Assembly, false);
        var canResolveEnumerable = typeManager.HasImplementations(typeof(IList<>));
        var enumerable = typeManager.Types.Where(x => x == typeof(IEnumerable<>)).ToArray();

        var properties = TypeHelper.ResolveProperties<B>(x => new {x.InnerOne.One, x.InnerTwo});

        var impl = typeManager.GetImplementations(typeof(MemberExpression));
        var result = typeof(ConstrainedComplex<,,,>).ResolveGenericArgumentsByImplementation(typeof(IGeneric<IGeneric<bool, IGeneric<bool, int>>>));
    }

    public static int Main(string[] args) => new Entrypoint()
        .Run(Run, args);


    private class B
    {
        public A InnerOne { get; set; } = null!;
        public A InnerTwo { get; set; } = null!;
    }

    private class A
    {
        public string One { get; set; } = null!;
        public string Two { get; set; } = null!;
    }
}