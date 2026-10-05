using Microsoft.EntityFrameworkCore;
using LabConnectPortal.Infra.Context;
using LabConnectPortal.Infra.Service.Interfaces;
using LabConnectPortal.Infra.Service.Repositories;

namespace LabConnectPortal
{
    public static class AddSamanehExtention
    {
        public static IServiceCollection AddSamaneh(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddCors(policyBuilder => policyBuilder.AddDefaultPolicy(policy => policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod()));

            services.AddDbContext<SamanehDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("SamanehConnection"), o => o.UseCompatibilityLevel(120));
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<ILabAgreement, LabAgreementRepository>();
            services.AddScoped<IReception, ReceptionRepository>();
            services.AddScoped<ISRLab, SRLabRepository>();

            services.AddHttpContextAccessor();

            return services;

        }
    }
}
