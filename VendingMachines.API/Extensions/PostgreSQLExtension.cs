using Microsoft.EntityFrameworkCore;
using VendingMachines.Infrastructure.Data;

namespace VendingMachines.API.Extensions
{
    public static class PostgreSQLExtension
    {
        public static void AddPostgreSQL(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<VendingMachinesContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("PostgresConnection")));
        }
    }
}
