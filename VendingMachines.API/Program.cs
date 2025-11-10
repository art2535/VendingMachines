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
                // ѕолитика CORS дл€ WPF-клиента
                // ѕозвол€ет любому источнику (любому домену) обращатьс€ к API.
                // »спользуетс€ дл€ десктопного приложени€, которое может быть на любом устройстве.
                options.AddPolicy("AllowWpf", builder =>
                {
                    builder.AllowAnyOrigin()   // –азрешить запросы с любого домена
                        .AllowAnyMethod()   // –азрешить любые HTTP-методы (GET, POST, PUT, DELETE и т.д.)
                        .AllowAnyHeader();  // –азрешить любые заголовки
                });

                // ѕолитика CORS дл€ Razor Pages веб-приложени€
                // –азрешает запросы только с конкретного веб-домена (https://localhost:7164)
                // ƒл€ безопасного взаимодействи€ браузера с API
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
