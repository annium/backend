using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using Annium.Core.Reflection;
using Annium.Core.Reflection.Tests.Types.Extensions.ResolveGenericArgumentsByImplentation;
using Annium.Core.Entrypoint;

namespace Demo.Core.Application
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var impl = TypeManager.Instance.GetImplementations(typeof(System.Linq.Expressions.MemberExpression));
            var result = typeof(ConstrainedComplex<, , ,>).ResolveGenericArgumentsByImplentation(typeof(IGeneric<IGeneric<bool, IGeneric<bool, int>>>));
        }

        public static int Main(string[] args) => new Entrypoint()
            .Run(Run, args);
    }
}