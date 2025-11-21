using VendingMachines.API.Extensions;

namespace VendingMachines.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddPostgreSQL(builder.Configuration);

            builder.Services.AddJwtAuthenticationWithoutEnvironment(builder.Configuration);

            builder.Services.AddSwagger();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowWpf", builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
                
                options.AddPolicy("AllowWebApp", policy =>
                {
                    policy.WithOrigins("https://localhost:7164")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });

                options.AddPolicy("AllowMobile", policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwaggerInterface();
            }

            app.UseCors("AllowWpf");
            app.UseCors("AllowWebApp");
            app.UseCors("AllowMobile");

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
