using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using RefillingStation.Api.Data;
using RefillingStation.Api.Entities;
using RefillingStation.Api.Features.CustomerDebts;
using RefillingStation.Api.Features.Expenses;
using RefillingStation.Api.Features.MontlyClosing.dtos;
using RefillingStation.Api.Features.Payrolls;
using RefillingStation.Api.Features.Trips;
using RefillingStation.Api.Features.Trips.dtos;
using RefillingStation.Api.Features.Trips.validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Fluent Validation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTripRequestValidator>();

// Services
builder.Services.AddScoped<TripImportService>();
builder.Services.AddScoped<ExpenseImportService>();
builder.Services.AddScoped<PayrollEntryImportService>();
builder.Services.AddScoped<CustomerDebtImportService>();

// Enable Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RefillingStation API",
        Version = "v1"
    });
});

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

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "RefillingStation.Api",
    utc = DateTime.UtcNow
}));

// Trips API
app.MapGet("/trips", async (AppDbContext db) =>
    await db.Trips
        .OrderByDescending(t => t.Date)
        .ThenBy(t => t.TripNumber)
        .ToListAsync()
);
app.MapGet("/trips/{id}", async (int id, AppDbContext db) =>
    await db.Trips.FindAsync(id) is Trip trip
        ? Results.Ok(trip)
        : Results.NotFound()
);
app.MapPost("/trips", async (CreateTripRequest request, 
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
});
app.MapPut("/trips/{id}", async (
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
});
app.MapDelete("/trips/{id}", async (int id, AppDbContext db) =>
{
    var trip = await db.Trips.FindAsync(id);
    if (trip is null) return Results.NotFound();

    db.Trips.Remove(trip);
    await db.SaveChangesAsync();

    return Results.NoContent();
});
app.MapPost("/trips/import", async (
        IFormFile file,
        TripImportService service) =>
{
    var result = await service.ImportAsync(file);
    return Results.Ok(result);
})
.DisableAntiforgery();

// Customer Debt Entries API
app.MapGet("/debt-entries", async (AppDbContext db) =>
    await db.CustomerDebtEntries
        .OrderByDescending(x => x.Date)
        .ToListAsync()
);

app.MapGet("/debt-entries/{id}", async (int id, AppDbContext db) =>
    await db.CustomerDebtEntries.FindAsync(id) is CustomerDebtEntry debt 
        ? Results.Ok(debt)
        : Results.NotFound()
);

app.MapPost("/debt-entries", async (CustomerDebtEntry debt, AppDbContext db) =>
{
    db.CustomerDebtEntries.Add(debt);
    await db.SaveChangesAsync();

    return Results.Created($"/debt-entries/{debt.Id}", debt);
});

app.MapPut("/debt-entries/{id}", async (int id, CustomerDebtEntry inputDebt, AppDbContext db) => {
    var debt = await db.CustomerDebtEntries.FindAsync(id);
    if (debt is null) return Results.NotFound();

    debt.Date = inputDebt.Date;
    debt.Amount = inputDebt.Amount;
    debt.CustomerName = inputDebt.CustomerName;
    debt.EntryType = inputDebt.EntryType;
    debt.RelatedTripId = inputDebt.RelatedTripId;
    debt.Notes = inputDebt.Notes;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/debt-entries/{id}", async (int id, AppDbContext db) =>
{
    var debt = await db.CustomerDebtEntries.FindAsync(id);
    if (debt is null) return Results.NotFound();

    db.CustomerDebtEntries.Remove(debt);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapPost("/debt-entries/import", async (IFormFile file, CustomerDebtImportService service) =>
{
    var result = await service.ImportAsync(file);

    return Results.Ok(result);
})
.DisableAntiforgery();

// Expenses API
app.MapGet("/expenses", async (AppDbContext db) => 
    await db.Expenses
        .OrderByDescending(x => x.Date)
        .ToListAsync()
);

app.MapGet("/expenses/{id}", async (int id, AppDbContext db) => 
    await db.Expenses.FindAsync(id) is Expense expense
        ? Results.Ok(expense)
        : Results.NotFound()
);

app.MapPost("/expenses", async (Expense expense, AppDbContext db) =>
{
    db.Expenses.Add(expense);
    await db.SaveChangesAsync();

    return Results.Created($"/debt-entries/{expense.Id}", expense);
});

app.MapPut("/expenses/{id}", async (int id, Expense inputExpense, AppDbContext db) =>
{
    var expense = await db.Expenses.FindAsync(id);
    if (expense is null) return Results.NotFound();

    expense.Date = inputExpense.Date;
    expense.ExpenseCategory = inputExpense.ExpenseCategory;
    expense.Amount = inputExpense.Amount;
    expense.Notes = inputExpense.Notes;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/expenses/{id}", async (int id, AppDbContext db) =>
{
    var expense = await db.Expenses.FindAsync(id);
    if (expense is null) return Results.NotFound();

    db.Expenses.Remove(expense);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapPost("/expenses/import", async (
    IFormFile file, 
    ExpenseImportService service) =>
{
    var result = await service.ImportAsync(file);
    return Results.Ok(result);
})
.DisableAntiforgery();

// Payroll
app.MapGet("/payroll-entries", async (AppDbContext db) =>
    await db.PayrollEntries
        .OrderByDescending(x => x.Date)
        .ToListAsync()
);

app.MapGet("/payroll-entries/{id}", async (int id, AppDbContext db) =>
    await db.PayrollEntries.FindAsync(id) is PayrollEntry payrollEntry
        ? Results.Ok(payrollEntry)
        : Results.NotFound()
);

app.MapPost("/payroll-entries", async(PayrollEntry payrollEntry, AppDbContext db) => 
{
    db.PayrollEntries.Add(payrollEntry);
    await db.SaveChangesAsync();

    return Results.Created($"/payroll-entries/{payrollEntry.Id}", payrollEntry);
});

app.MapPut("/payroll-entries/{id}", async(int id, PayrollEntry inputPayrollEntry, AppDbContext db) =>
{
    var payrollEntry = await db.PayrollEntries.FindAsync(id);
    if (payrollEntry is null) return Results.NotFound();

    payrollEntry.Date = inputPayrollEntry.Date;
    payrollEntry.EmployeeName = inputPayrollEntry.EmployeeName;
    payrollEntry.SalaryAmount = inputPayrollEntry.SalaryAmount;
    payrollEntry.AdvanceGiven = inputPayrollEntry.AdvanceGiven;
    payrollEntry.AdvanceDeduction = inputPayrollEntry.AdvanceDeduction;
    payrollEntry.CashPaid = inputPayrollEntry.CashPaid;
    payrollEntry.Notes = inputPayrollEntry.Notes;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/payroll-entries/{id}", async (int id, AppDbContext db) =>
{
    var payrollEntry = await db.PayrollEntries.FindAsync(id);
    if (payrollEntry is null) return Results.NotFound();

    db.PayrollEntries.Remove(payrollEntry);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapPost("/payroll-entries/import", async (IFormFile file, PayrollEntryImportService service) =>
{
    var result = await service.ImportAsync(file);
    return Results.Ok(result);
})
.DisableAntiforgery();

// Dashboard API
app.MapGet("/daily-summary/{date}", async (
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

app.MapGet("/dashboard", async (
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
});

app.MapGet("/monthly-summary", async (
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
});

app.MapPost("/monthly-summary", async (MonthlyClosingRequestDto request, AppDbContext db) =>
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
});

app.Run();
