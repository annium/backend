using System;
using System.ComponentModel;
using System.Threading;
using Annium.Core.Entrypoint;
using Annium.Extensions.Primitives;

namespace Demo.Extensions.Primitives
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var v = "a,b".ParseFlags<TestEnum>(',');

            var str = "07DC22";
            var result = str.TryFromHexStringToByteArray(out var byteArray);
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);


        [Flags]
        private enum TestEnum
        {
            [Description("empty")]
            None,
            [Description("a")]
            One,
            [Description("b")]
            Two
        }
    }
}