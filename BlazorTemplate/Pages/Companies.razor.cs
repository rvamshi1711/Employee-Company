using BlazorTemplate.Data;
using BlazorTemplate.Data.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.EntityFrameworkCore;

namespace BlazorTemplate.Pages;

public partial class Companies : ComponentBase
{
    [Inject] private IDbContextFactory<ApplicationDbContext> DbFactory { get; set; } = default!;
    [Inject] private ILogger<Companies> Logger { get; set; } = default!;

    private QuickGrid<Company> _dataGrid = default!;
    private GridItemsProvider<Company> _provider = default!;
    private int _totalItemsCount;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await UpdateWorkerCountForAllCompanies();

        _provider = async request =>
        {
            var result = new GridItemsProviderResult<Company>()
            {
                Items = [],
                TotalItemCount = 0
            };

            try
            {
                Logger.LogInformation("Loading companies...");
                await using var db = await DbFactory.CreateDbContextAsync(request.CancellationToken);

                var query = db.Companies
                    .AsNoTracking()
                    .Include(c => c.Workers)
                    .Skip(request.StartIndex);

                if (request.Count.HasValue)
                {
                    query = query.Take(request.Count.Value);
                }

                var totalCount = await db.Companies.CountAsync(request.CancellationToken);

                if (totalCount != _totalItemsCount && !request.CancellationToken.IsCancellationRequested)
                {
                    _totalItemsCount = totalCount;
                    StateHasChanged();
                }

                result = new GridItemsProviderResult<Company>
                {
                    Items = await request.ApplySorting(query).ToListAsync(request.CancellationToken),
                    TotalItemCount = totalCount
                };
            }
            catch (OperationCanceledException)
            {
                // Ignored
            }

            return result;
        };
    }

    private async Task UpdateWorkerCountForAllCompanies()
    {
        await using var db = await DbFactory.CreateDbContextAsync();

        var companies = await db.Companies.ToListAsync();

        foreach (var company in companies)
        {
            company.WorkerCount = await db.Workers
                .CountAsync(w => w.AssignedCompanyId == company.CompanyId);

            Logger.LogInformation("Updated WorkerCount for company '{}': {}", company.Name, company.WorkerCount);
        }

        await db.SaveChangesAsync();
    }

    private async Task DeleteCompanyAsync(int companyId)
    {
        Logger.LogInformation("Deleting company with id {}", companyId);

        await using var db = await DbFactory.CreateDbContextAsync();

        // Get the company and its workers
        var company = await db.Companies
            .Include(c => c.Workers)
            .FirstOrDefaultAsync(c => c.CompanyId == companyId);

        if (company != null)
        {
            // Nullify the foreign key for workers associated with the company
            foreach (var worker in company.Workers)
            {
                worker.AssignedCompanyId = null;
            }

            // Save changes to workers
            await db.SaveChangesAsync();

            // Delete the company record
            var deletedCount = await db.Companies
                .Where(c => c.CompanyId == companyId)
                .ExecuteDeleteAsync();

            if (deletedCount != 0)
            {
                Logger.LogInformation("Successfully deleted company {}", companyId);

                // Recalculate WorkerCount for the company (if applicable)
                company.WorkerCount = await db.Workers
                    .CountAsync(w => w.AssignedCompanyId == company.CompanyId);

                await db.SaveChangesAsync();
                Logger.LogInformation("Updated WorkerCount for company '{}': {}", company.Name, company.WorkerCount);


                // Force a refresh of the data grid
                if (_dataGrid is not null)
                {
                    await _dataGrid.RefreshDataAsync();
                }

                // Refresh the entire page to show the updated data
                StateHasChanged();
            }
            else
            {
                Logger.LogWarning("Company with id {} could not be deleted. Does it exist?", companyId);
            }
        }
    }

}