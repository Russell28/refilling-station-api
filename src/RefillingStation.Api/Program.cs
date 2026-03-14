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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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
    trip.ToBePaidQty = inputTrip.ToBePaidQty;
    trip.ActualPaidQty = inputTrip.ActualPaidQty;

    trip.ReturnedQty = inputTrip.ReturnedQty;
    trip.ReplacementQty = inputTrip.ReplacementQty;

    trip.PricePerGallon = inputTrip.PricePerGallon;
    trip.ActualCashCollected = inputTrip.ActualCashCollected;

    trip.RelatedTripId = inputTrip.RelatedTripId;
    trip.AdjustmentReason = inputTrip.AdjustmentReason;

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

app.Run();
