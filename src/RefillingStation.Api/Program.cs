using BCrypt.Net;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using RefillingStation.Api.Data;
using RefillingStation.Api.Entities;
using RefillingStation.Api.Features.Auth;
using RefillingStation.Api.Features.Auth.dtos;
using RefillingStation.Api.Features.CustomerDebts;
using RefillingStation.Api.Features.CustomerDebts.dtos;
using RefillingStation.Api.Features.Expenses;
using RefillingStation.Api.Features.Expenses.dtos;
using RefillingStation.Api.Features.MontlyClosing.dtos;
using RefillingStation.Api.Features.Payrolls;
using RefillingStation.Api.Features.Payrolls.dtos;
using RefillingStation.Api.Features.Trips;
using RefillingStation.Api.Features.Trips.dtos;
using RefillingStation.Api.Features.Trips.validators;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Fluent Validation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTripRequestValidator>();

// Services
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<TripImportService>();
builder.Services.AddScoped<ExpenseImportService>();
builder.Services.AddScoped<PayrollEntryImportService>();
builder.Services.AddScoped<CustomerDebtImportService>();

// Enable Swagger
builder.Services.AddEndpointsApiExplorer(); // minimal API explorer for Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RefillingStation API",
        Version = "v1"
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Authentication & Authorization
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// Validate required configuration
if (string.IsNullOrEmpty(jwtKey))
    throw new InvalidOperationException("Jwt:Key configuration is missing. Set environment variable: Jwt__Key");
if (string.IsNullOrEmpty(jwtIssuer))
    throw new InvalidOperationException("Jwt:Issuer configuration is missing. Set environment variable: Jwt__Issuer");
if (string.IsNullOrEmpty(jwtAudience))
    throw new InvalidOperationException("Jwt:Audience configuration is missing. Set environment variable: Jwt__Audience");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing. Set environment variable: ConnectionStrings__DefaultConnection");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

var app = builder.Build();

// Middleware Pipeline - Start
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //api.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Serve React build files - for monolithic or static hosting
//app.UseDefaultFiles();
//app.UseStaticFiles();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();
// Middleware Pipeline - End

var api = app.MapGroup("/api");

// Map Endpoints
api.MapGet("/ping", () => "API is alive");

api.MapGet("/auth/me", (ClaimsPrincipal user) =>
{
    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var username = user.Identity?.Name;
    var role = user.FindFirst(ClaimTypes.Role)?.Value;

    return Results.Ok(new
    {
        userId,
        username,
        role
    });
})
.RequireAuthorization();

// Auth API
api.MapPost("/auth/login", async (
    LoginRequest request, 
    TokenService service,
    AppDbContext db) =>
{
    if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        return Results.BadRequest("Username and Password are required.");

    var user = await db.Users
        .FirstOrDefaultAsync(u => u.Username == request.Username);

    if (user is null)
        return Results.Unauthorized();

    var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

    if (!isPasswordValid)
        return Results.Unauthorized();

    var token = service.GenerateToken(user);

    return Results.Ok(new
    {
        token,
        username = user.Username,
        role = user.Role
    });
});

// Trips API
api.MapGet("/trips", async (AppDbContext db) =>
    await db.Trips
        .OrderByDescending(t => t.Date)
        .ThenBy(t => t.TripNumber)
        .Take(50)
        .ToListAsync()
)
.RequireAuthorization();

api.MapGet("/trips/{id}", async (int id, AppDbContext db) =>
    await db.Trips.FindAsync(id) is Trip trip
        ? Results.Ok(trip)
        : Results.NotFound()
).RequireAuthorization();

api.MapPost("/trips", async (CreateTripRequest request, 
    IValidator<CreateTripRequest> validator,
    AppDbContext db) =>
{
    var validationResult = await validator.ValidateAsync(request);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(
            validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                )
        );
    }

    // Check for duplicate
    var exists = await db.Trips.AnyAsync(x =>
        x.Date == DateOnly.FromDateTime(request.Date)
        && x.TripNumber == request.TripNumber);

    if (exists)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["TripNumber"] = new[] { "Trip number already exists for this date." }
        });
    }

    var trip = new Trip
    {
        Date = DateOnly.FromDateTime(request.Date),
        TripNumber = request.TripNumber,
        TimeStarted = request.TimeStarted,
        TimeEnded = request.TimeEnded,
        EmployeeName = request.EmployeeName,
        Source = request.Source,
        TripType = request.TripType,
        CustomerCategory = request.CustomerCategory,
        CollectedQty = request.CollectedQty,
        LoadedQty = request.LoadedQty,
        DeliveredQty = request.DeliveredQty,
        FreeQty = request.FreeQty,
        ReturnedQty = request.ReturnedQty,
        ReplacementQty = request.ReplacementQty,
        ActualCashCollected = request.ActualCashCollected,
        Notes = request.Notes
    };

    


    db.Trips.Add(trip);
    await db.SaveChangesAsync();

    return Results.Created($"/trips/{trip.Id}", trip);
})
.RequireAuthorization();

api.MapPut("/trips/{id}", async (
    int id, 
    UpdateTripRequest request, 
    IValidator<UpdateTripRequest> validator,
    AppDbContext db) =>
{
    var validationResult = await validator.ValidateAsync(request);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(
            validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                )
        );
    }

    // Check for duplicate
    var exists = await db.Trips.AnyAsync(x =>
        x.Id != id // exclude self
        && x.Date == DateOnly.FromDateTime(request.Date)
        && x.TripNumber == request.TripNumber);

    if (exists)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["TripNumber"] = new[] { "Trip number already exists for this date." }
        });
    }

    var trip = await db.Trips.FindAsync(id);

    if (trip is null) 
        return Results.NotFound();

    // Update all properties
    trip.Date = DateOnly.FromDateTime(request.Date);
    trip.TripNumber = request.TripNumber;
    trip.TimeStarted = request.TimeStarted;
    trip.TimeEnded = request.TimeEnded;
    trip.EmployeeName = request.EmployeeName;
    trip.Source = request.Source;
    trip.TripType = request.TripType;
    trip.CustomerCategory = request.CustomerCategory;
    trip.CollectedQty = request.CollectedQty;
    trip.LoadedQty = request.LoadedQty;
    trip.DeliveredQty = request.DeliveredQty;
    trip.FreeQty = request.FreeQty;
    trip.ReturnedQty = request.ReturnedQty;
    trip.ReplacementQty = request.ReplacementQty;
    trip.ActualCashCollected = request.ActualCashCollected;
    trip.IsRemitted = request.IsRemitted;
    trip.Notes = request.Notes;

    await db.SaveChangesAsync();

    return Results.NoContent();
})
.RequireAuthorization();

api.MapDelete("/trips/{id}", async (int id, AppDbContext db) =>
{
    var trip = await db.Trips.FindAsync(id);
    if (trip is null) return Results.NotFound();

    db.Trips.Remove(trip);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.RequireAuthorization();

api.MapPost("/trips/import", async (
        IFormFile file,
        TripImportService service) =>
{
    var result = await service.ImportAsync(file);
    return Results.Ok(result);
})
.DisableAntiforgery()
.RequireAuthorization("AdminOnly");

api.MapGet("/trips/next-trip-number", async (DateOnly date, AppDbContext db) =>
{
    // Default next trip number
    var nextTripNo = 1;

    var trips = await db.Trips
        .Where(x => x.Date == date)
        .ToListAsync();

    if (trips.Any())
    {
        nextTripNo = trips.Max(x => x.TripNumber) + 1;
    }

    var result = new
    {
        date = date,
        nextTripNo = nextTripNo
    };

    return Results.Ok(result);

});

// Customer Debt Entries API
api.MapGet("/debt-entries", async (AppDbContext db) =>
    await db.CustomerDebtEntries
        .OrderByDescending(x => x.Date)
        .Take(50)
        .ToListAsync()
)
.RequireAuthorization();

api.MapGet("/debt-entries/{id}", async (int id, AppDbContext db) =>
    await db.CustomerDebtEntries.FindAsync(id) is CustomerDebtEntry debt 
        ? Results.Ok(debt)
        : Results.NotFound()
)
.RequireAuthorization();

api.MapPost("/debt-entries", async (CreateDebtRequest request, IValidator<CreateDebtRequest> validator, AppDbContext db) =>
{
    var validationResult = await validator.ValidateAsync(request);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(
            validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                )
        );
    }

    var debt = new CustomerDebtEntry
    {
        Date = request.Date,
        CustomerName = request.CustomerName,
        Amount = request.Amount,
        Notes = request.Notes
    };

    db.CustomerDebtEntries.Add(debt);
    await db.SaveChangesAsync();

    return Results.Created($"/debt-entries/{debt.Id}", debt);
})
.RequireAuthorization();

api.MapPut("/debt-entries/{id}", async (int id, CreateDebtRequest request, IValidator <CreateDebtRequest> validator, AppDbContext db) => {
    var validationResult = await validator.ValidateAsync(request);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(
            validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                )
        );
    }

    var debt = await db.CustomerDebtEntries.FindAsync(id);
    if (debt is null) return Results.NotFound();

    debt.Date = request.Date;
    debt.Amount = request.Amount;
    debt.CustomerName = request.CustomerName;
    debt.Notes = request.Notes;

    await db.SaveChangesAsync();
    return Results.NoContent();
})
.RequireAuthorization();

api.MapDelete("/debt-entries/{id}", async (int id, AppDbContext db) =>
{
    var debt = await db.CustomerDebtEntries.FindAsync(id);
    if (debt is null) return Results.NotFound();

    db.CustomerDebtEntries.Remove(debt);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.RequireAuthorization();

api.MapPost("/debt-entries/import", async (IFormFile file, CustomerDebtImportService service) =>
{
    var result = await service.ImportAsync(file);

    return Results.Ok(result);
})
.DisableAntiforgery()
.RequireAuthorization("AdminOnly");

// Expenses API
api.MapGet("/expenses", async (AppDbContext db) => 
    await db.Expenses
        .OrderByDescending(x => x.Date)
        .Take(50)
        .ToListAsync()
)
.RequireAuthorization();

api.MapGet("/expenses/{id}", async (int id, AppDbContext db) => 
    await db.Expenses.FindAsync(id) is Expense expense
        ? Results.Ok(expense)
        : Results.NotFound()
)
.RequireAuthorization();

api.MapPost("/expenses", async (CreateExpenseRequest request, IValidator<CreateExpenseRequest> validator, AppDbContext db) =>
{
    var validationResult = await validator.ValidateAsync(request);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(
            validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                )
        );
    }

    var newExpense = new Expense()
    {
        Date = request.Date,
        ExpenseCategory = request.ExpenseCategory,
        Amount = request.Amount,
        Notes = request.Notes
    };

    db.Expenses.Add(newExpense);
    await db.SaveChangesAsync();

    return Results.Created($"/expenses/{newExpense.Id}", newExpense);
})
.RequireAuthorization();

api.MapPut("/expenses/{id}", async (int id, Expense inputExpense, AppDbContext db) =>
{
    var expense = await db.Expenses.FindAsync(id);
    if (expense is null) return Results.NotFound();

    expense.Date = inputExpense.Date;
    expense.ExpenseCategory = inputExpense.ExpenseCategory;
    expense.Amount = inputExpense.Amount;
    expense.Notes = inputExpense.Notes;

    await db.SaveChangesAsync();
    return Results.NoContent();
})
.RequireAuthorization();

api.MapDelete("/expenses/{id}", async (int id, AppDbContext db) =>
{
    var expense = await db.Expenses.FindAsync(id);
    if (expense is null) return Results.NotFound();

    db.Expenses.Remove(expense);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.RequireAuthorization();

api.MapPost("/expenses/import", async (
    IFormFile file, 
    ExpenseImportService service) =>
{
    var result = await service.ImportAsync(file);
    return Results.Ok(result);
})
.DisableAntiforgery()
.RequireAuthorization("AdminOnly");

// Payroll
api.MapGet("/payroll-entries", async (AppDbContext db) =>
    await db.PayrollEntries
        .OrderByDescending(x => x.Date)
        .Take(50)
        .ToListAsync()
)
.RequireAuthorization("AdminOnly");

api.MapGet("/payroll-entries/{id}", async (int id, AppDbContext db) =>
    await db.PayrollEntries.FindAsync(id) is PayrollEntry payrollEntry
        ? Results.Ok(payrollEntry)
        : Results.NotFound()
)
.RequireAuthorization("AdminOnly");

api.MapPost("/payroll-entries", async(CreatePayrollRequest request, IValidator<CreatePayrollRequest> validator, AppDbContext db) => 
{
    var validationResult = await validator.ValidateAsync(request);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(
            validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                )
        );
    }

    var newPayroll = new PayrollEntry()
    {
        Date = request.Date,
        EmployeeName = request.EmployeeName,
        SalaryAmount = request.SalaryAmount,
        AdvanceGiven = request.AdvanceGiven,
        AdvanceDeduction = request.AdvanceDeduction,
        CashPaid = request.CashPaid,
        Notes = request.Notes
    };

    db.PayrollEntries.Add(newPayroll);
    await db.SaveChangesAsync();

    return Results.Created($"/payroll-entries/{newPayroll.Id}", request);
})
.RequireAuthorization("AdminOnly");

api.MapPut("/payroll-entries/{id}", async(int id, CreatePayrollRequest request, IValidator<CreatePayrollRequest> validator, AppDbContext db) =>
{
    var validationResult = await validator.ValidateAsync(request);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(
            validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                )
        );
    }

    var payrollEntry = await db.PayrollEntries.FindAsync(id);
    if (payrollEntry is null) return Results.NotFound();

    payrollEntry.Date = request.Date;
    payrollEntry.EmployeeName = request.EmployeeName;
    payrollEntry.SalaryAmount = request.SalaryAmount;
    payrollEntry.AdvanceGiven = request.AdvanceGiven;
    payrollEntry.AdvanceDeduction = request.AdvanceDeduction;
    payrollEntry.CashPaid = request.CashPaid;
    payrollEntry.Notes = request.Notes;

    await db.SaveChangesAsync();
    return Results.NoContent();
})
.RequireAuthorization("AdminOnly");

api.MapDelete("/payroll-entries/{id}", async (int id, AppDbContext db) =>
{
    var payrollEntry = await db.PayrollEntries.FindAsync(id);
    if (payrollEntry is null) return Results.NotFound();

    db.PayrollEntries.Remove(payrollEntry);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.RequireAuthorization("AdminOnly");

api.MapPost("/payroll-entries/import", async (IFormFile file, PayrollEntryImportService service) =>
{
    var result = await service.ImportAsync(file);
    return Results.Ok(result);
})
.DisableAntiforgery()
.RequireAuthorization("AdminOnly");

// Dashboard API
api.MapGet("/daily-summary/{date}", async (
    DateTime date, 
    AppDbContext db,
    IConfiguration config) =>
{
    var targetDate = DateOnly.FromDateTime(date);
    var targetDateTime = targetDate.ToDateTime(TimeOnly.MinValue);
    var openingBacklogQty = config.GetValue<Decimal>("BacklogSettings:OpeningBacklogQty");

    var trips = await db.Trips
        .Where(x => x.Date == targetDate)
        .ToListAsync();

    var expenses = await db.Expenses
        .Where(x => x.Date.Date == targetDateTime.Date)
        .ToListAsync();

    var payrolls = await db.PayrollEntries
        .Where(x => x.Date.Date == targetDateTime.Date)
        .ToListAsync();

    var debtToday = await db.CustomerDebtEntries
        .Where(x => x.Date.Date == targetDateTime.Date)
        .ToListAsync();

    var runningDebt = await db.CustomerDebtEntries
        .Where(x => x.Date.Date <= targetDateTime.Date)
        .ToListAsync();

    // Previous
    var previousTrips = await db.Trips
        .Where(x => x.Date < targetDate)
        .Select(x => new
        {
            x.CollectedQty,
            x.DeliveredQty
        })
        .ToListAsync();
    var previousTotalCollectedQty = previousTrips.Sum(x => x.CollectedQty);
    var previousTotalDeliveredQty = previousTrips.Sum(x => x.DeliveredQty);

    var backlogStartQty = openingBacklogQty + previousTotalCollectedQty - previousTotalDeliveredQty;

    var totalCollectedQty = trips.Sum(x => x.CollectedQty);
    var totalLoadedQty = trips.Sum(x => x.LoadedQty);
    var totalDeliveredQty = trips.Sum(x => x.DeliveredQty);
    var totalFreeQty = trips.Sum(x => x.FreeQty);
    var totalReturnedQty = trips.Sum(x => x.ReturnedQty);
    var totalReplacementQty = trips.Sum(x => x.ReplacementQty);

    var backlogEndQty = backlogStartQty + totalCollectedQty - totalDeliveredQty;

    var totalCashCollected = trips.Sum(x => x.ActualCashCollected);
    var totalExpenses = expenses.Sum(x => x.Amount);
    var totalPayrollPaid = payrolls.Sum(x => x.CashPaid);


    var result = new
    {
        Date = targetDate,
        TripCount = trips.Count,

        BacklogStartQty = backlogStartQty,
        TotalCollectedQty = totalCollectedQty,
        TotalLoadedQty = totalLoadedQty,
        TotalDeliveredQty = totalDeliveredQty,
        BacklogEndQty = backlogEndQty,

        TotalFreeQty = totalFreeQty,
        TotalReturnedQty = totalReturnedQty,
        TotalReplacementQty = totalReplacementQty,

        TotalCashCollected = totalCashCollected,
        TotalExpenses = totalExpenses,
        TotalPayrollPaid = totalPayrollPaid,

        TotalDebtCreatedToday = debtToday.Where(x => x.Amount > 0).Sum(x => x.Amount),
        TotalDebtPaymentsToday = debtToday.Where(x => x.Amount < 0).Sum(x => Math.Abs(x.Amount)),
        OutstandingDebt = runningDebt.Sum(x => x.Amount),

        NetCashFlow = totalCashCollected - totalExpenses - totalPayrollPaid
    };

    return Results.Ok(result);
});

api.MapGet("/dashboard", async (
    DateTime startDate, 
    DateTime endDate, 
    AppDbContext db,
    IConfiguration config) =>
{
    var dateOnlyStart = DateOnly.FromDateTime(startDate);
    var dateOnlyEnd = DateOnly.FromDateTime(endDate);

    // Within start and end dates
    var trips = await db.Trips
        .Where(x => 
            x.Date >= dateOnlyStart
            && x.Date <= dateOnlyEnd)
        .ToListAsync();

    var expenses = await db.Expenses
        .Where(x => x.Date >= startDate && x.Date <= endDate)
        .ToListAsync();

    var payrolls = await db.PayrollEntries
        .Where(x => x.Date >= startDate && x.Date <= endDate)
        .ToListAsync();

    var debts = await db.CustomerDebtEntries
        .Where(x => x.Date >= startDate && x.Date <= endDate)
        .ToListAsync();

    // Before start date
    var tripsBefore = await db.Trips
        .Where(x => x.Date < dateOnlyStart)
        .ToListAsync();

    var debtsRunning = await db.CustomerDebtEntries
        .Where(x => x.Date <= endDate)
        .ToListAsync();

    var payrollRunning = await db.PayrollEntries
        .Where(x => x.Date <= endDate)
        .ToListAsync();

    // Compute Summary (CORE)

    // Backlog Start
    var openingBacklogQty = config.GetValue<decimal>("BacklogSettings:OpeningBacklogQty");

    var previousCollected = tripsBefore.Sum(x => x.CollectedQty);
    var previousDelivered = tripsBefore.Sum(x => x.DeliveredQty);

    var backlogStartQty = openingBacklogQty + previousCollected - previousDelivered;

    // Operations
    var totalTrips = trips.Count;
    var totalCollectedQty = trips.Sum(x => x.CollectedQty);
    var totalDeliveredQty = trips.Sum(x => x.DeliveredQty);
    var totalLoadedQty = trips.Sum(x => x.LoadedQty);

    var backlogEndQty = backlogStartQty + totalCollectedQty - totalDeliveredQty;

    // Cash
    var totalCashCollected = trips.Sum(x => x.ActualCashCollected);
    var totalExpenses = expenses.Sum(x => x.Amount);
    var totalPayrollPaid = payrolls.Sum(x => x.CashPaid);

    var netCashFlow = totalCashCollected - totalExpenses - totalPayrollPaid;

    // Debt
    var totalDebtCreated = debts.Where(x => x.Amount > 0).Sum(x => x.Amount);
    var totalDebtPayments = debts.Where(x => x.Amount < 0).Sum(x => Math.Abs(x.Amount));

    var outstandingDebt = debtsRunning.Sum(x => x.Amount);

    // Payroll
    var totalSalaryEarned = payrolls.Sum(x => x.SalaryAmount);
    var payrollPaid = payrolls.Sum(x => x.CashPaid);

    var payrollOwed = totalSalaryEarned - payrollPaid;

    var outstandingPayroll = payrollRunning.Sum(x => x.SalaryAmount)
        - payrollRunning.Sum(x => x.CashPaid);

    // DAILY Report
    var dailyReports = new List<object>();

    for (var date = startDate; date <= endDate; date = date.AddDays(1))
    {
        var tripDateOnly = DateOnly.FromDateTime(date);

        var tripsPerDay = trips.Where(x => x.Date == tripDateOnly).ToList();
        var expensesPerDay = expenses.Where(x => x.Date == date).ToList();
        var payrollsPerDay = payrolls.Where(x => x.Date == date).ToList();
        var debtsPerDay = debts.Where(x => x.Date == date).ToList();

        var collected = tripsPerDay.Sum(x => x.CollectedQty);
        var delivered = tripsPerDay.Sum(x => x.DeliveredQty);
        var cashTotal = tripsPerDay.Sum(x => x.ActualCashCollected);
        var payrollPaidTotal = payrollsPerDay.Sum(x => x.CashPaid);
        var expenseTotal = expensesPerDay.Sum(x => x.Amount);

        backlogStartQty += collected - delivered;

        dailyReports.Add(new
        {
            date,
            tripCount = tripsPerDay.Count(),
            collectedQty = collected,
            deliveredQty = delivered,
            cashCollected = cashTotal,
            expenses = expenseTotal,
            payrollPaid = payrollPaidTotal,
            debtCreated = debtsPerDay.Where(x => x.Amount > 0).Sum(x => x.Amount),
            debtPayments = debtsPerDay.Where(x => x.Amount < 0).Sum(x => Math.Abs(x.Amount)),
            netCashFlow = cashTotal - expenseTotal - payrollPaidTotal,
            backlogEndQty = backlogStartQty
        });
    }

    // Expense Breakdown
    var expenseBreakdown = expenses
        .GroupBy(x => x.ExpenseCategory)
        .Select(g => new
        {
            category = g.Key,
            amount = g.Sum(x => x.Amount),
        })
        .ToList();

    // Debt Breakdown
    var debtBreakdown = debts
        .GroupBy(x => x.CustomerName)
        .Select(g => new
        {
            customerName = g.Key,
            debtCreated = g.Where(x => x.Amount > 0).Sum(x => x.Amount),
            debtPayments = g.Where(x => x.Amount < 0).Sum(x => Math.Abs(x.Amount)),
            balance = g.Sum(x => x.Amount)
        })
        .ToList();

    // Payroll Breakdown
    var payrollBreakdown = payrolls
        .GroupBy(x => x.EmployeeName)
        .Select(g => new
        {
            employeeName = g.Key,
            salaryEarned = g.Sum(x => x.SalaryAmount),
            cashPaid = g.Sum(x => x.CashPaid),
            balance = g.Sum(x => x.SalaryAmount) - g.Sum(x => x.CashPaid)
        })
        .ToList();



    return Results.Ok(new
    {
        summary = new
        {
            totalTrips,
            backlogStartQty,
            totalCollectedQty,
            totalLoadedQty,
            totalDeliveredQty,
            backlogEndQty,

            totalCashCollected,
            totalExpenses,
            totalPayrollPaid,
            netCashFlow,

            totalDebtCreated,
            totalDebtPayments,
            outstandingDebt,

            totalSalaryEarned,
            payrollPaid,
            payrollOwed,
            outstandingPayroll
        },
        dailyReports,
        expenseBreakdown,
        debtBreakdown,
        payrollBreakdown,
    });
})
.RequireAuthorization("AdminOnly");

api.MapGet("/monthly-summary", async (
    string month, AppDbContext db) =>
{
    if (String.IsNullOrEmpty(month))
    {
        return Results.BadRequest("Month is required. Use format yyyy-MM.");
    }

    if (!DateOnly.TryParse($"{month}-01", out var firstDayOfMonth))
    {
        return Results.BadRequest("Invalid month format. Use format yyyy-MM.");
    }

    var firstDayDateTime = firstDayOfMonth.ToDateTime(TimeOnly.MinValue);
    var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
    var lastDayDateTime = lastDayOfMonth.ToDateTime(TimeOnly.MaxValue);

    var trips = await db.Trips
        .Where(x => x.Date >= firstDayOfMonth &&  x.Date <= lastDayOfMonth)
        .ToListAsync();

    var expenses = await db.Expenses
        .Where(x => x.Date >= firstDayDateTime && x.Date <= lastDayDateTime)
        .ToListAsync();

    //var debts = await db.CustomerDebtEntries
    //    .Where(x => x.Date >= firstDayDateTime && x.Date <= lastDayDateTime)
    //    .ToListAsync();

    var payrolls = await db.PayrollEntries
        .Where(x => x.Date >= firstDayDateTime && x.Date <= lastDayDateTime)
        .ToListAsync();

    var totalCashCollected = trips.Sum(x => x.ActualCashCollected);
    var totalExpenses = expenses.Sum(x => x.Amount);
    var totalPayrollEarned = payrolls.Sum(x => x.SalaryAmount);

    var netProfit = totalCashCollected - totalExpenses - totalPayrollEarned;

    var savedClosing = await db.MonthlyClosings
        .Where(x => x.Month == month)
        .Select(x => new
        {
            x.ManagerShare,
            x.OwnerShare,
            x.Notes,
            x.CreatedAt
        })
        .FirstOrDefaultAsync();

    var result = new
    {
        Month = month,
        TotalCashCollected = totalCashCollected,
        TotalExpenses = totalExpenses,
        TotalPayrollEarned = totalPayrollEarned,
        NetProfit = netProfit,
        SavedClosing = savedClosing
    };

    return Results.Ok(result);
})
.RequireAuthorization("AdminOnly");

api.MapPost("/monthly-summary", async (MonthlyClosingRequestDto request, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Month))
    {
        return Results.BadRequest("Month is required. Use format yyyy-MM.");
    }

    if (!DateOnly.TryParse($"{request.Month}-01", out var firstDayOfMonth))
    {
        return Results.BadRequest("Invalid month format. Use format yyyy-MM.");
    }

    if (request.ManagerShare < 0)
    {
        return Results.BadRequest("Manager share cannot be negative.");
    }

    if (request.OwnerShare < 0)
    {
        return Results.BadRequest("Owner share cannot be negative.");
    }

    var firstDayDateTime = firstDayOfMonth.ToDateTime(TimeOnly.MinValue);
    var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
    var lastDayDateTime = lastDayOfMonth.ToDateTime(TimeOnly.MaxValue);

    var trips = await db.Trips
        .Where(x => x.Date >= firstDayOfMonth && x.Date <= lastDayOfMonth)
        .ToListAsync();

    var expenses = await db.Expenses
        .Where(x => x.Date >= firstDayDateTime && x.Date <= lastDayDateTime)
        .ToListAsync();

    var payrolls = await db.PayrollEntries
        .Where(x => x.Date >= firstDayDateTime && x.Date <= lastDayDateTime)
        .ToListAsync();

    var totalCashCollected = trips.Sum(x => x.ActualCashCollected);
    var totalExpenses = expenses.Sum(x => x.Amount);
    var totalPayrollEarned = payrolls.Sum(x => x.SalaryAmount);

    var netProfit = totalCashCollected - totalExpenses - totalPayrollEarned;

    if (netProfit < 0 && (request.ManagerShare > 0 || request.OwnerShare > 0))
    {
        return Results.BadRequest("Cannot assign profit shares when net profit is negative.");
    }

    var totalShare = request.ManagerShare + request.OwnerShare;

    if (totalShare > netProfit)
    {
        return Results.BadRequest("Total shares cannot be greater than net profit.");
    }

    var existingClosing = await db.MonthlyClosings
        .FirstOrDefaultAsync(x => x.Month == request.Month);

    if (existingClosing is null)
    {
        var monthlyClosing = new MonthlyClosing
        {
            Month = request.Month,
            TotalCashCollected = totalCashCollected,
            TotalExpenses = totalExpenses,
            TotalPayrollEarned = totalPayrollEarned,
            NetProfit = netProfit,
            ManagerShare = request.ManagerShare,
            OwnerShare = request.OwnerShare,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        db.MonthlyClosings.Add(monthlyClosing);
    }
    else
    {
        existingClosing.TotalCashCollected = totalCashCollected;
        existingClosing.TotalExpenses = totalExpenses;
        existingClosing.TotalPayrollEarned = totalPayrollEarned;
        existingClosing.NetProfit = netProfit;
        existingClosing.ManagerShare = request.ManagerShare;
        existingClosing.OwnerShare = request.OwnerShare;
        existingClosing.Notes = request.Notes;
    }

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        Message = "Monthly summary saved successfully.",
        Month = request.Month,
        TotalCashCollected = totalCashCollected,
        TotalExpenses = totalExpenses,
        TotalPayrollEarned = totalPayrollEarned,
        NetProfit = netProfit,
        ManagerShare = request.ManagerShare,
        OwnerShare = request.OwnerShare,
        RemainingBalance = netProfit - totalShare
    });
})
.RequireAuthorization("AdminOnly");

// Seed DB
await SeedAdminUserAsync(app.Services);
static async Task SeedAdminUserAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        await db.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine("MIGRATION ERROR: " + ex.Message);
        throw;
    }

    // Admin
    var adminExists = await db.Users.AnyAsync(u => u.Username == "admin");

    if (!adminExists)
    {
        var adminUser = new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = "Admin"
        };
        db.Users.Add(adminUser);
    }

    // Employee
    var employeeExists = await db.Users.AnyAsync(u => u.Username == "employee");

    if (!employeeExists)
    {
        var employeeUser = new User
        {
            Username = "employee",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employee123!"),
            Role = "Employee"
        };

        db.Users.Add(employeeUser);
        await db.SaveChangesAsync();
    }

    if (!adminExists || !employeeExists)
        await db.SaveChangesAsync();
}

// Fallback for React client-side routes - for monolithic or static hosting
//api.MapFallbackToFile("index.html"); 

app.Run();
