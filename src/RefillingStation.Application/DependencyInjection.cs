using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RefillingStation.Application.Interfaces;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Application.Services;

namespace RefillingStation.Application
{
    public static class DependencyInjection
    {
        // This method is used to register application services in the dependency injection container.
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ICustomerDebtService, CustomerDebtService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IExpenseCategoryService, ExpenseCategoryService>();
            services.AddScoped<IExpenseService, ExpenseService>();
            services.AddScoped<IPayrollService, PayrollService>();
            services.AddScoped<ITripService, TripService>();
            services.AddScoped<IReportsService, ReportsService>();
            services.AddScoped<IMonthlyClosingService, MonthlyClosingService>();
            services.AddScoped<IUserService, UserService>();

            // Validators
            services.AddValidatorsFromAssembly(
                    typeof(DependencyInjection).Assembly);

            return services;
        }
    }
}
