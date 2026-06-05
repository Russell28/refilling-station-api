using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RefillingStation.Api.Middleware;
using RefillingStation.Application;
using RefillingStation.Application.Settings;
using RefillingStation.Infrastructure;
using RefillingStation.Infrastructure.Persistence;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

//
// ------------------------------------------------------------
// 1. Configure Services (Dependency Injection)
// ------------------------------------------------------------
//

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // This converter makes enums serialize/deserialize as strings
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Application + Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// App settings
builder.Services.Configure<BacklogSettings>(
    builder.Configuration.GetSection("BacklogSettings"));

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
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
        policy.WithOrigins("https://localhost:5173")
              .AllowCredentials() // critical — without it, cookies won’t be accepted.
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

//
// ------------------------------------------------------------
// 2. Authentication & Authorization
// ------------------------------------------------------------
//

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

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

//
// ------------------------------------------------------------
// 3. Middleware Pipeline
// ------------------------------------------------------------
//

// Development-only tools
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Database initialization (runs once at startup)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.InitializeAsync(db);
}

// Security + Routing middleware
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

//
// ------------------------------------------------------------
// 4. Run Application
// ------------------------------------------------------------
//

app.Run();
