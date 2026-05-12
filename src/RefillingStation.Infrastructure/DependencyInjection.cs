using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using RefillingStation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Infrastructure.Persistence.Repositories;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Infrastructure.Authentication;

namespace RefillingStation.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            // Repositories
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<ICustomerDebtRepository, CustomerDebtRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IExpenseCategoryRepository, ExpenseCategoryRepository>();
            services.AddScoped<IExpenseRepository, ExpenseRepository>();
            services.AddScoped<IPayrollRepository, PayrollRepository>();
            services.AddScoped<ITripRepository, TripRepository>();

            // Services
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

            return services;
        }
    }
}
