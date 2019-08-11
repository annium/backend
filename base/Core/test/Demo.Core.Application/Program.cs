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
            // implementation = typeof(int).GetImplementationOf(typeof(System.ValueType));
            target = typeof(IGenericConstrained<,>).MakeGenericType(typeof(BasePlain), typeof(GenericPlain<>));
            implementation = typeof(OpenComplex<long>).GetImplementationOf(target);
            // implementation = typeof(OpenComplex<long>).GetImplementationOf(typeof(IGenericConstrained<,>).MakeGenericType(typeof(BasePlain), typeof(OtherComplex<>)));
            // implementation = typeof(OpenComplex<int>).GetImplementationOf(typeof(ComplexPlain<>));
            // implementation = typeof(IGeneric<,>).GetImplementationOf(typeof(IPlain));
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);

        private class OtherComplete : OtherComplex<bool>, IGeneric<bool, string> { }

        private class OtherComplex<T> : BasePlain { }

        private class OtherPlain { }

        private class OpenComplex<T> : ComplexPlain<T>, IGenericConstrained<BasePlain, GenericPlain<T>> { }

        private class CompleteComplex : ComplexPlain<bool>, IGenericConstrained<BasePlain, GenericPlain<bool>> { }

        private class ComplexPlain<T> : GenericPlain<T>, IGeneric<T, int> { }

        private class GenericPlain<T> : BasePlain { }

        private class BasePlain : IPlain { }

        private interface IPlain { }

        private interface IGeneric<T1, T2> { }

        private interface IGenericConstrained<T1, T2> where T2 : class, T1, new() where T1 : IPlain { }
    }
}