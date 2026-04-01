using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using RefillingStation.Api.Data;
using RefillingStation.Api.Entities;
using RefillingStation.Api.Features.Expenses;
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

app.Run();
