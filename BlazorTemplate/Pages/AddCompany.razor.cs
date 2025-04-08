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

    [Parameter] public int? CompanyId { get; set; }

    protected Company _model = new();
    protected bool _isEditMode => CompanyId.HasValue;

    protected override async Task OnInitializedAsync()
    {
        if (_isEditMode)
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            var existingCompany = await db.Companies.FindAsync(CompanyId.Value);

            if (existingCompany is not null)
            {
                _model = existingCompany;
            }
        }
    }

    protected async Task SubmitCompanyAsync()
    {
        try
        {
            await using var db = await DbFactory.CreateDbContextAsync();

            if (_isEditMode)
            {
                var existingCompany = await db.Companies.FindAsync(CompanyId.Value);

                if (existingCompany is not null)
                {
                    existingCompany.Name = _model.Name;
                    existingCompany.City = _model.City;
                    existingCompany.State = _model.State;
                    existingCompany.ZipCode = _model.ZipCode;

                    await db.SaveChangesAsync();
                    Logger.LogInformation("Updated company: {CompanyName}", _model.Name);
                }
            }
            else
            {
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
            }

            Navigation.NavigateTo("/companies");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving company.");
        }
    }
}
