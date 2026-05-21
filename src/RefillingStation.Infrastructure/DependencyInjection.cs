using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using RefillingStation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Infrastructure.Persistence.Repositories;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Infrastructure.Authentication;
using RefillingStation.Application.Interfaces.Repositories.Reports;
using RefillingStation.Infrastructure.Persistence.Repositories.Reports;
using RefillingStation.Application.Interfaces.Services.Imports;
using RefillingStation.Infrastructure.Imports;

namespace RefillingStation.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            // DB
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

            // Repositories
            services.AddScoped<ICustomerDebtRepository, CustomerDebtRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IExpenseCategoryRepository, ExpenseCategoryRepository>();
            services.AddScoped<IExpenseRepository, ExpenseRepository>();
            services.AddScoped<IPayrollRepository, PayrollRepository>();
            services.AddScoped<ITripRepository, TripRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();
            services.AddScoped<IDailySummaryRepository, DailySummaryRepository>();
            services.AddScoped<IMonthlySummaryRepository, MonthlySummaryRepository>();
            services.AddScoped<IMonthlyClosingRepository, MonthlyClosingRepository>();

            // Services
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

            services.AddScoped<IExpenseImportService, ExpenseImportService>();
            services.AddScoped<ICustomerDebtImportService, CustomerDebtImportService>();
            services.AddScoped<IPayrollImportService, PayrollImportService>();
            services.AddScoped<ITripImportService, TripImportService>();

            return services;
        }
    }
}
