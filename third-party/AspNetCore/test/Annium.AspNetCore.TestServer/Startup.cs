using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.AspNetCore.TestServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceContainer container)
        {
            container.Collection.AddControllers();
            container.Collection.AddCors();
            container.Collection.AddMvc()
                .AddDefaultJsonOptions();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseExceptionMiddleware();
            app.UseRouting();
            app.UseCors(builder => builder
                .SetIsOriginAllowed(o => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}