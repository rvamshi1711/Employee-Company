using BlazorTemplate.Data;
using BlazorTemplate.Data.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BlazorTemplate.Pages;

public partial class AddCompany : ComponentBase
{
    [Inject] private IDbContextFactory<ApplicationDbContext> DbFactory { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<AddCompany> Logger { get; set; } = default!;

    protected Company _model = new();

    protected async Task SubmitCompanyAsync()
    {
        try
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            db.Companies.Add(new Company
            {
                Name = _model.Name,
                City = _model.City,
                State = _model.State,
                ZipCode = _model.ZipCode,
                WorkerCount = 0
            });

            await db.SaveChangesAsync();
            Logger.LogInformation("Added new company: {CompanyName}", _model.Name);

            Navigation.NavigateTo("/companies");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error adding company.");
        }
    }
}
