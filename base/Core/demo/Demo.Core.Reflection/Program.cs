using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Annium.Core.Entrypoint;
using Annium.Core.Reflection;
using Annium.Core.Reflection.Tests.Types.Extensions.ResolveGenericArgumentsByImplementation;
using Annium.Core.Runtime.Types;

await using var entry = Entrypoint.Default.Setup();

var typeManager = TypeManager.GetInstance(typeof(Program).Assembly);
var canResolveEnumerable = typeManager.HasImplementations(typeof(IList<>));
var enumerable = typeManager.Types.Where(x => x == typeof(IEnumerable<>)).ToArray();

var properties = TypeHelper.ResolveProperties<B>(x => new { x.InnerOne.One, x.InnerTwo });

var impl = typeManager.GetImplementations(typeof(MemberExpression));
var result = typeof(ConstrainedComplex<,,,>).ResolveGenericArgumentsByImplementation(typeof(IGeneric<IGeneric<bool, IGeneric<bool, int>>>));

class B
{
    public A InnerOne { get; set; } = null!;
    public A InnerTwo { get; set; } = null!;
}

class A
{
    public string One { get; set; } = null!;
    public string Two { get; set; } = null!;
}