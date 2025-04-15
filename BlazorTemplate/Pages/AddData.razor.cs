using BlazorTemplate.Data;
using BlazorTemplate.Data.Entities;
using BlazorTemplate.Ext;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BlazorTemplate.Pages;

public partial class AddData : ComponentBase
{
    [Inject] private IDbContextFactory<ApplicationDbContext> DbFactory { get; set; } = default!;
    [Inject] private ILogger<AddData> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavManager { get; set; } = default!;

    protected string? StatusMessage;
    private Company[] _companies = default!;

    protected override async Task OnInitializedAsync()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        _companies = await db.Companies.AsNoTracking().ToArrayAsync();
        Logger.LogInformation("Loaded {Count} companies", _companies.Length);
    }

    protected async Task SeedWorkersAsync()
    {
        if (_companies is null || _companies.Length == 0)
        {
            StatusMessage = "No companies available in the database.";
            return;
        }

        var random = new Random();
        await using var db = await DbFactory.CreateDbContextAsync();

        for (int i = 0; i < 5; i++)
        {
            var company = _companies[random.Next(_companies.Length)];

            var worker = new Worker
            {
                FirstName = RandomFirstName(random),
                LastName = RandomLastName(random),
                Email = $"worker{i}{random.Next(100, 999)}@example.com",
                PhoneNumber = $"12345678{random.Next(10, 99)}".FormatAsPhoneNumber(),
                BirthDate = RandomBirthDate(random),
                AssignedCompanyId = company.CompanyId
            };

            await db.Workers.AddAsync(worker);
            await db.SaveChangesAsync();

            var updatedCompany = await db.Companies.FirstOrDefaultAsync(c => c.CompanyId == worker.AssignedCompanyId);
            if (updatedCompany is not null)
            {
                updatedCompany.WorkerCount = await db.Workers
                    .Where(w => w.AssignedCompanyId == updatedCompany.CompanyId)
                    .CountAsync();

                await db.SaveChangesAsync();
                Logger.LogInformation("Updated WorkerCount for '{}': {}", updatedCompany.Name, updatedCompany.WorkerCount);
            }
        }

        StatusMessage = "5 random workers added successfully with live WorkerCount updates.";
        Logger.LogInformation("Seeded 5 workers and updated company counts.");
    }

    private static string RandomFirstName(Random random) =>
        new[] { "Alice", "Bob", "Charlie", "Diana", "Edward", "Fiona", "George", "Hannah" }[random.Next(8)];

    private static string RandomLastName(Random random) =>
        new[] { "Smith", "Johnson", "Brown", "Taylor", "Anderson", "Thomas", "White", "Martin" }[random.Next(8)];

    private static DateOnly RandomBirthDate(Random random)
    {
        int year = random.Next(1960, 2000);
        int month = random.Next(1, 13);
        int day = random.Next(1, 28);
        return new DateOnly(year, month, day);
    }

    private sealed class Model
    {
        public int? AssignedCompanyId { get; set; }

        [Required, Range(typeof(DateOnly), "01/01/1920", "01/01/2020")]
        public DateOnly? BirthDate { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required, Phone, MinLength(10), MaxLength(14)]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
