using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RentalApplications.Domain;
using RentalApplications.Domain.Entities;

namespace RentalApplications.Data;

public static class DataSeeder
{
    public const string DefaultManagerEmail = "manager@example.com";
    public const string DefaultApplicantEmail = "applicant@example.com";
    public const string DefaultPassword = "Password123!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DataSeeder));
        var db = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await EnsureRolesAsync(roleManager);
        await EnsureUserAsync(userManager, DefaultManagerEmail, "Property Manager", AppRoles.PropertyManager, logger);
        var applicant = await EnsureUserAsync(userManager, DefaultApplicantEmail, "Sample Applicant", AppRoles.Applicant, logger);

        // Sample data is seeded once; roles and default users are ensured on every startup.
        if (await db.UnitTypes.AnyAsync())
        {
            logger.LogInformation("Sample rental data already present; skipping Bogus seed.");
            return;
        }

        logger.LogInformation("Seeding sample properties, units, and applications.");

        var unitTypes = new List<UnitType>
        {
            new() { Name = "Studio", IsActive = true },
            new() { Name = "One Bedroom", IsActive = true },
            new() { Name = "Two Bedroom", IsActive = true },
            new() { Name = "Loft", IsActive = false }
        };
        db.UnitTypes.AddRange(unitTypes);
        await db.SaveChangesAsync();

        var propertyFaker = new Faker<Property>()
            .RuleFor(p => p.Name, f => $"{f.Address.StreetName()} Apartments")
            .RuleFor(p => p.StreetAddress, f => f.Address.StreetAddress())
            .RuleFor(p => p.City, f => f.Address.City())
            .RuleFor(p => p.State, f => f.Address.StateAbbr())
            .RuleFor(p => p.PostalCode, f => f.Address.ZipCode());

        var properties = propertyFaker.Generate(4);
        db.Properties.AddRange(properties);
        await db.SaveChangesAsync();

        var unitFaker = new Faker<Unit>()
            .RuleFor(u => u.UnitNumber, f => f.Random.Replace("###"))
            .RuleFor(u => u.MonthlyRent, f => f.Finance.Amount(900, 2800, 0));

        var units = new List<Unit>();
        foreach (var property in properties)
        {
            for (var i = 0; i < 4; i++)
            {
                var unitType = unitTypes[fakerIndex(i)];
                var unit = unitFaker.Generate();
                unit.PropertyId = property.Id;
                unit.UnitTypeId = unitType.Id;
                unit.Bedrooms = BedroomsForSeedUnitType(unitType.Name);
                units.Add(unit);
            }
        }

        var loftType = unitTypes[3];
        units[0].UnitTypeId = loftType.Id;
        units[0].Bedrooms = BedroomsForSeedUnitType(loftType.Name);
        db.Units.AddRange(units);
        await db.SaveChangesAsync();

        var extraApplicants = new List<ApplicationUser>();
        for (var i = 0; i < 3; i++)
        {
            var email = $"applicant{i + 2}@example.com";
            extraApplicants.Add(await EnsureUserAsync(userManager, email, $"Applicant {i + 2}", AppRoles.Applicant, logger));
        }

        var allApplicants = new[] { applicant, extraApplicants[0], extraApplicants[1], extraApplicants[2] };
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await SeedApplicationAsync(db, units[1], allApplicants[0], ApplicationStatus.Draft, today);
        await SeedApplicationAsync(db, units[2], allApplicants[1], ApplicationStatus.Submitted, today);
        await SeedApplicationAsync(db, units[3], allApplicants[2], ApplicationStatus.Returned, today);
        await SeedApplicationAsync(db, units[4], allApplicants[0], ApplicationStatus.Denied, today);
        await SeedApplicationAsync(db, units[5], allApplicants[1], ApplicationStatus.Withdrawn, today);

        var approvedApp = await SeedApplicationAsync(db, units[6], allApplicants[2], ApplicationStatus.Approved, today);
        var leaseStart = today.AddMonths(-2);
        db.Leases.Add(new Lease
        {
            UnitId = units[6].Id,
            RentalApplicationId = approvedApp.Id,
            StartDate = leaseStart,
            EndDate = leaseStart.AddMonths(LeaseTerms.StandardLeaseMonths)
        });
        await db.SaveChangesAsync();

        logger.LogInformation("Sample data seed completed.");

        static int fakerIndex(int i) => i % 3;

        static int BedroomsForSeedUnitType(string unitTypeName) => unitTypeName switch
        {
            "Studio" => 0,
            "One Bedroom" => 1,
            "Two Bedroom" => 2,
            "Loft" => 1,
            _ => 1
        };
    }

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { AppRoles.Applicant, AppRoles.PropertyManager })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string displayName,
        string role,
        ILogger logger)
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
            await userManager.CreateAsync(user, DefaultPassword);
            await userManager.AddToRoleAsync(user, role);
            logger.LogInformation("Created seed user {Email} with role {Role}", email, role);
        }

        return user;
    }

    private static async Task<RentalApplication> SeedApplicationAsync(
        AppDbContext db,
        Unit unit,
        ApplicationUser applicant,
        ApplicationStatus status,
        DateOnly today)
    {
        var faker = new Faker();
        var application = new RentalApplication
        {
            UnitId = unit.Id,
            ApplicantUserId = applicant.Id,
            Status = status,
            CurrentSection = ApplicationSection.Summary,
            ApplicantInformationSaved = true,
            ResidenceHistorySaved = true,
            ApplicantName = applicant.DisplayName,
            Phone = faker.Phone.PhoneNumber(),
            Email = applicant.Email!,
            CurrentStreetAddress = faker.Address.StreetAddress(),
            CurrentCity = faker.Address.City(),
            CurrentState = faker.Address.StateAbbr(),
            CurrentPostalCode = faker.Address.ZipCode()
        };

        application.ResidenceHistories.Add(new ResidenceHistory
        {
            StreetAddress = faker.Address.StreetAddress(),
            City = faker.Address.City(),
            State = faker.Address.StateAbbr(),
            PostalCode = faker.Address.ZipCode(),
            LandlordName = faker.Name.FullName(),
            LandlordPhone = faker.Phone.PhoneNumber(),
            MoveInDate = today.AddYears(-2),
            MoveOutDate = today.AddYears(-1)
        });

        db.RentalApplications.Add(application);
        await db.SaveChangesAsync();

        if (status == ApplicationStatus.Draft)
        {
            db.ApplicationStatusHistories.Add(new ApplicationStatusHistory
            {
                RentalApplicationId = application.Id,
                FromStatus = ApplicationStatus.Draft,
                ToStatus = ApplicationStatus.Draft,
                ChangedByUserId = applicant.Id,
                ChangedAt = DateTimeOffset.UtcNow.AddDays(-5)
            });
        }
        else
        {
            db.ApplicationStatusHistories.Add(new ApplicationStatusHistory
            {
                RentalApplicationId = application.Id,
                FromStatus = ApplicationStatus.Draft,
                ToStatus = ApplicationStatus.Submitted,
                ChangedByUserId = applicant.Id,
                ChangedAt = DateTimeOffset.UtcNow.AddDays(-5),
                Comment = "Submitted for review."
            });
        }

        if (status is ApplicationStatus.Returned or ApplicationStatus.Denied or ApplicationStatus.Approved)
        {
            var manager = await db.Users.FirstAsync(u => u.Email == DefaultManagerEmail);
            db.ApplicationStatusHistories.Add(new ApplicationStatusHistory
            {
                RentalApplicationId = application.Id,
                FromStatus = ApplicationStatus.Submitted,
                ToStatus = status,
                ChangedByUserId = manager.Id,
                ChangedAt = DateTimeOffset.UtcNow.AddDays(-2),
                Comment = status switch
                {
                    ApplicationStatus.Returned => "Please update residence history.",
                    ApplicationStatus.Denied => "Income requirements not met.",
                    ApplicationStatus.Approved => "Approved.",
                    _ => null
                }
            });
        }

        if (status == ApplicationStatus.Withdrawn)
        {
            db.ApplicationStatusHistories.Add(new ApplicationStatusHistory
            {
                RentalApplicationId = application.Id,
                FromStatus = ApplicationStatus.Submitted,
                ToStatus = ApplicationStatus.Withdrawn,
                ChangedByUserId = applicant.Id,
                ChangedAt = DateTimeOffset.UtcNow.AddDays(-1),
                Comment = "Withdrawn by applicant."
            });
        }

        await db.SaveChangesAsync();
        return application;
    }
}

