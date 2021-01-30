using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.AspNetCore.TestServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddCors();
            services.AddMvc()
                .AddDefaultJsonOptions();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseExceptionMiddleware();
            // app.UseWebSocketsMiddleware();
            app.UseRouting();
            app.UseCors(builder => builder
                .SetIsOriginAllowed(_ => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}