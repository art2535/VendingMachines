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

            builder.Services.AddJwtAuthentication(builder.Configuration);

            builder.Services.AddSwagger();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwaggerInterface();
            }

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
