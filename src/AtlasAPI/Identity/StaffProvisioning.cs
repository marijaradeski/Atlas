using AtlasInfrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace AtlasAPI.Identity;

public static class StaffProvisioning
{
    public static async Task RunAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<AtlasStaffUser>>();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        Console.WriteLine("ATLAS - Staff Account Provisioning");
        Console.WriteLine("----------------------------------");

        Console.Write("Full name: ");
        var fullName = Console.ReadLine()?.Trim();

        Console.Write("Email: ");
        var email = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email))
        {
            Console.WriteLine("Full name and email are required.");
            return;
        }

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            Console.WriteLine("A staff account with this email already exists.");
            return;
        }

        Console.Write("Password: ");
        var password = ReadPassword();

        Console.Write("\nConfirm password: ");
        var confirmation = ReadPassword();

        Console.WriteLine();

        if (password != confirmation)
        {
            Console.WriteLine("Passwords do not match.");
            return;
        }

        const string roleName = "ComplianceStaff";

        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await roleManager.CreateAsync(
                new IdentityRole<Guid>(roleName));

            if (!roleResult.Succeeded)
            {
                PrintErrors(roleResult.Errors);
                return;
            }
        }

        var user = new AtlasStaffUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, password);

        if (!createResult.Succeeded)
        {
            PrintErrors(createResult.Errors);
            return;
        }

        var roleAssignment = await userManager.AddToRoleAsync(
            user,
            roleName);

        if (!roleAssignment.Succeeded)
        {
            await userManager.DeleteAsync(user);
            Console.WriteLine("Role assignment failed. The new user was removed.");
            PrintErrors(roleAssignment.Errors);
            return;
        }

        Console.WriteLine();
        Console.WriteLine("Staff account created successfully.");
        Console.WriteLine($"Staff ID: {user.Id}");
        Console.WriteLine($"Email: {user.Email}");
        Console.WriteLine($"Role: {roleName}");
    }

    private static string ReadPassword()
    {
        if (Console.IsInputRedirected)
            throw new InvalidOperationException("Password provisioning requires an interactive console.");

        var password = new System.Text.StringBuilder();

        while (true)
        {
            var key = Console.ReadKey(intercept: true);

            if (key.Key == ConsoleKey.Enter)
                break;

            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                    password.Length--;

                continue;
            }

            if (!char.IsControl(key.KeyChar))
                password.Append(key.KeyChar);
        }

        return password.ToString();
    }

    private static void PrintErrors(IEnumerable<IdentityError> errors)
    {
        foreach (var error in errors)
            Console.WriteLine($"- {error.Description}");
    }
}
