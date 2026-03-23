using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using RefillingStation.Api.Data;
using RefillingStation.Api.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

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
app.MapPost("/trips", async (Trip trip, AppDbContext db) =>
{
    db.Trips.Add(trip);
    await db.SaveChangesAsync();
    return Results.Created($"/trips/{trip.Id}", trip);
});
app.MapPut("/trips/{id}", async (int id, Trip inputTrip, AppDbContext db) =>
{
    var trip = await db.Trips.FindAsync(id);
    if (trip is null) return Results.NotFound();

    // Update all properties
    trip.Date = inputTrip.Date;
    trip.TripNumber = inputTrip.TripNumber;
    trip.Segment = inputTrip.Segment;

    trip.TimeStarted = inputTrip.TimeStarted;
    trip.TimeEnded = inputTrip.TimeEnded;

    trip.Source = inputTrip.Source;
    trip.TripType = inputTrip.TripType;
    trip.EmployeeName = inputTrip.EmployeeName;
    trip.CustomerCategory = inputTrip.CustomerCategory;

    trip.CollectedQty = inputTrip.CollectedQty;
    trip.LoadedQty = inputTrip.LoadedQty;
    trip.DeliveredQty = inputTrip.DeliveredQty;

    trip.FreeQty = inputTrip.FreeQty;
    //trip.ToBePaidQty = inputTrip.ToBePaidQty;
    trip.ActualPaidQty = inputTrip.ActualPaidQty;
    trip.ReturnedQty = inputTrip.ReturnedQty;
    trip.ReplacementQty = inputTrip.ReplacementQty;

    //trip.PricePerGallon = inputTrip.PricePerGallon;
    trip.ActualCashCollected = inputTrip.ActualCashCollected;
    trip.IsRemitted = inputTrip.IsRemitted;
    trip.RelatedTripId = inputTrip.RelatedTripId;
    trip.Notes = inputTrip.Notes;

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

// Dashboard API
app.MapGet("/daily-summary/{date}", async (DateTime date, AppDbContext db) =>
{
    var targetDate = date.Date;

    var trips = await db.Trips
        .Where(x => x.Date.Date == targetDate)
        .ToListAsync();

    var expenses = await db.Expenses
        .Where(x => x.Date.Date == targetDate)
        .ToListAsync();

    var payrolls = await db.PayrollEntries
        .Where(x => x.Date.Date == targetDate)
        .ToListAsync();

    var debtToday = await db.CustomerDebtEntries
        .Where(x => x.Date.Date == targetDate)
        .ToListAsync();

    var runningDebt = await db.CustomerDebtEntries
        .Where(x => x.Date.Date <= targetDate)
        .ToListAsync();

    var totalCashCollected = trips.Sum(x => x.ActualCashCollected);
    var totalExpenses = expenses.Sum(x => x.Amount);
    var totalPayrollPaid = payrolls.Sum(x => x.CashPaid);

    var result = new
    {
        Date = targetDate,
        TripCount = trips.Count,

        TotalCollectedQty = trips.Sum(x => x.CollectedQty),
        TotalLoadedQty = trips.Sum(x => x.LoadedQty),
        TotalDeliveredQty = trips.Sum(x => x.DeliveredQty),
        TotalFreeQty = trips.Sum(x => x.FreeQty),
        //TotalToBePaidQty = trips.Sum(x => x.ToBePaidQty),
        TotalActualPaidQty = trips.Sum(x => x.ActualPaidQty),
        TotalReturnedQty = trips.Sum(x => x.ReturnedQty),
        TotalReplacementQty = trips.Sum(x => x.ReplacementQty),

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
