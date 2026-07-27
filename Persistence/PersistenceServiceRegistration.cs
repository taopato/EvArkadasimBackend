using Application.Services.Repositories;
using Core.Interfaces;
using Core.Security.JWT;                  // ITokenHelper
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Persistence.Repositories;
using Persistence.Security.JWT;
using Persistence.Services;            // EfUserRepository

namespace Persistence
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AddPersistenceServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(opt =>
               opt.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddSingleton<ITokenHelper, JwtHelper>();
            services.AddScoped<IUserRepository, EfUserRepository>();
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<IVerificationCodeRepository, EfVerificationCodeRepository>();
            services.AddScoped<IExpenseRepository, EfExpenseRepository>();
            services.AddScoped<IHouseRepository, EfHouseRepository>();
            services.AddScoped<IPaymentRepository, EfPaymentRepository>();
            services.AddScoped<IShareRepository, EfShareRepository>();
            services.AddScoped<IPaymentAllocationRepository, EfPaymentAllocationRepository>();
            services.AddScoped<IPaymentReadRepository, EfPaymentReadRepository>();
            services.AddScoped<IExpenseReadRepository, EfExpenseReadRepository>();
            services.AddScoped<ILedgerLineRepository, EfLedgerLineRepository>();



            return services;
        }
    }

}
