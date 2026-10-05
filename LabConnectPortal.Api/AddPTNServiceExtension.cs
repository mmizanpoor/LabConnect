using Microsoft.EntityFrameworkCore;
using LabConnectPortal.Infra.Context;
using LabConnectPortal.Infra.Service.Interfaces;
using LabConnectPortal.Infra.Service.Repositories;

namespace LabConnectPortal
{
    public static class AddPTNServiceExtension
    {
        public static IServiceCollection AddPTNService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PTNServiceDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("sql2008Connection"), o => o.UseCompatibilityLevel(120));
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            services.AddScoped<ITestECL, TestECLRepository>();

            services.AddHttpContextAccessor();

            return services;

        }
    }
}
