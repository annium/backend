using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.AspNetCore
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddCors();
            services.AddMvc()
                .AddNewtonsoftJson(opts => opts.SerializerSettings.ConfigureForOperations());
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseCors(builder => builder
                .SetIsOriginAllowed(o => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}