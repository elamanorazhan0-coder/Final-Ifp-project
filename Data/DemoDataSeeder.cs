using AutoCare.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Data;

public static class DemoDataSeeder
{
    public const string AdministratorRole = "Administrator";
    public const string MechanicRole = "Mechanic";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var factory = services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

        foreach (var role in new[] { AdministratorRole, MechanicRole })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        var admin = await FindOrCreateUser(userManager, "admin@autocare.local", "AutoCare Admin", "AutoCare123!", AdministratorRole);
        var mechanic = await FindOrCreateUser(userManager, "mechanic@autocare.local", "Alex Mechanic", "AutoCare123!", MechanicRole);

        await using var db = await factory.CreateDbContextAsync();
        if (await db.Customers.AnyAsync())
            return;

        var customer = new Customer { Name = "Jordan Lee", Email = "jordan@example.local", Phone = "+1 555 0100" };
        var vehicle = new Vehicle
        {
            Customer = customer,
            Make = "Toyota",
            Model = "Corolla",
            Year = 2021,
            RegistrationNumber = "AC-2021"
        };
        var appointment = new Appointment
        {
            Customer = customer,
            Vehicle = vehicle,
            RequestedService = "Brake inspection",
            StartsAt = DateTime.Today.AddDays(1).AddHours(10),
            DurationMinutes = 60,
            Status = AppointmentStatus.Confirmed,
            MechanicId = mechanic.Id
        };
        var part = new Part { Name = "Brake pads", Sku = "BRK-001", Quantity = 4, MinimumStock = 5, UnitPrice = 48.50m };
        db.AddRange(customer, vehicle, appointment, part);
        db.RepairOrders.Add(new RepairOrder
        {
            Vehicle = vehicle,
            MechanicId = mechanic.Id,
            Status = RepairStatus.InProgress,
            Diagnosis = "Front brake pads worn",
            WorkDescription = "Inspect and replace front brake pads",
            LaborCost = 85m
        });
        await db.SaveChangesAsync();
    }

    private static async Task<ApplicationUser> FindOrCreateUser(
        UserManager<ApplicationUser> userManager, string email, string displayName, string password, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = displayName
            };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Could not create demo account {email}: {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }
        if (!await userManager.IsInRoleAsync(user, role))
        {
            var result = await userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Could not assign role to {email}: {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }
        return user;
    }
}
