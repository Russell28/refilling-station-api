using Microsoft.EntityFrameworkCore;
using RefillingStation.Domain.Entities;
using RefillingStation.Domain.Enums;

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
                (
                    "admin",
                    BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    UserRole.Admin
                ));
            }

            // Employee
            if (!await db.Users.AnyAsync(u => u.Username == "employee"))
            {
                db.Users.Add(new User
                (
                    "employee",
                    BCrypt.Net.BCrypt.HashPassword("Employee123!"),
                    UserRole.Employee
                ));
            }

            await db.SaveChangesAsync();
        }
    }
}
