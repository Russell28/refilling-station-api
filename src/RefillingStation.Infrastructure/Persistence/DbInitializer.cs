using Microsoft.EntityFrameworkCore;
using RefillingStation.Domain.Entities;

namespace RefillingStation.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(AppDbContext db)
        {
            await db.Database.MigrateAsync();

            // Admin
            if (!await db.Users.AnyAsync(u => u.Username == "admin"))
            {
                db.Users.Add(new User
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Role = "Admin"
                });
            }

            // Employee
            if (!await db.Users.AnyAsync(u => u.Username == "employee"))
            {
                db.Users.Add(new User
                {
                    Username = "employee",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employee123!"),
                    Role = "Employee"
                });
            }

            await db.SaveChangesAsync();
        }
    }
}
