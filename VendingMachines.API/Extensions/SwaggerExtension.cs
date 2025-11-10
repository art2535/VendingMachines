using Microsoft.OpenApi.Models;

namespace VendingMachines.API.Extensions
{
    public static class SwaggerExtension
    {
        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Vending Machines API",
                    Version = "v1",
                    Description = "API для управления торговыми автоматами"
                });
                options.EnableAnnotations();
            });
        }

        public static void UseSwaggerInterface(this IApplicationBuilder builder)
        {
            builder.UseSwagger();
            builder.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });
        }
    }
}
