using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.DependencyInjection.Obsolete;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Demo.Core.WebAssembly
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            builder.ConfigureContainer(new ServiceProviderFactory(x => x.UseServicePack<ServicePack>()));
            await builder.Build().RunAsync();
        }
    }
}