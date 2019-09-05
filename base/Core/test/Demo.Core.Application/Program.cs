using System;
using System.Threading;
using Annium.Core.Application.Types;
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
            Type target = null;
            Type implementation = null;
            target = typeof(IGeneric<int, bool>);
            // var arguments = typeof(MultiComplex<,>).ResolveGenericArgumentsByImplentations(typeof(OtherComplex<bool>), typeof(IGeneric<long, int>));
            // implementation = typeof(Other<>).ResolveByImplentations(typeof(IGeneric<int, bool>));
            // implementation = typeof(MultiComplex<,>).ResolveByImplentations(typeof(OtherComplex<bool>), typeof(IGeneric<long, int>));
            // implementation = typeof(ValueTuple<int, bool>).GetTargetImplementation(typeof(ValueTuple<,>));
            implementation = typeof(ConstrainedComplex<, , ,>).ResolveByImplentations(typeof(IOther<IGeneric<bool, IGeneric<bool, int>>>));
            // implementation = typeof(MultiComplex<,>).GetGenericArguments() [0].ResolveByImplentations(typeof(OtherComplex<bool>));
            // var ownInterfaces = typeof(OpenComplex<int>).GetOwnInterfaces();
            // implementation = typeof(int).GetImplementationOf(typeof(System.ValueType));
            // implementation = typeof(OpenComplex<long>).GetImplementationOf(typeof(IGenericConstrained<,>).MakeGenericType(typeof(BasePlain), typeof(OtherComplex<>)));
            // implementation = typeof(OpenComplex<int>).GetImplementationOf(typeof(ComplexPlain<>));
            // implementation = typeof(IGeneric<,>).GetImplementationOf(typeof(IPlain));
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);

        private class Other<T> : Next<int, T> { }

        private class Next<T1, T2> : Derived<T2, T1> { }

        private class Derived<T1, T2> : Base<T2, T1> { }

        private class Base<T1, T2> : IGeneric<T1, T2> { }

        private class OtherComplete : OtherComplex<bool>, IGeneric<bool, string> { }

        private class OtherComplex<T> : BasePlain { }

        private class OtherPlain { }

        private class ConstrainedComplex<T1, T2, T3, T4> : IOther<T1> where T1 : IGeneric<T2, T3> where T3 : IGeneric<T2, T4> { }

        private class MultiComplex<T1, T2> : OtherComplex<T1>, IGeneric<T2, int> { }

        private class OpenComplex<T> : ComplexPlain<T>, IGenericConstrained<BasePlain, GenericPlain<T>>, IPlain { }

        private class CompleteComplex : ComplexPlain<bool>, IGenericConstrained<BasePlain, GenericPlain<bool>> { }

        private class ComplexPlain<T> : GenericPlain<T>, IGeneric<T, int> { }

        private class GenericPlain<T> : BasePlain { }

        private class BasePlain : IPlain { }

        private interface IPlain { }

        private interface IGeneric<T1, T2> { }

        private interface IOther<T> { }

        private interface IGenericConstrained<T1, T2> where T2 : class, T1, new() where T1 : IPlain { }
    }
}