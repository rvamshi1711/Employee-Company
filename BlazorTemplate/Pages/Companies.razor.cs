

using BlazorTemplate.Data;
using BlazorTemplate.Data.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.EntityFrameworkCore;

namespace BlazorTemplate.Pages;

public partial class Companies(IDbContextFactory<ApplicationDbContext> dbFactory, ILogger<Companies> logger)
{
    // TODO: Implement logic matching that of AddWorker.razor.cs except for the company table.
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory = dbFactory;
    private readonly ILogger<Companies> _logger = logger;

    private QuickGrid<Company> _dataGrid = default!;
    private GridItemsProvider<Company> _provider = default!;
    private int _totalItemsCount;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        await UpdateWorkerCountForAllCompanies();
        _provider = async request =>
        {
            var providerResult = new GridItemsProviderResult<Company>()
            {
                Items = [],
                TotalItemCount = 0
            };

            try
            {
                _logger.LogInformation("Loading companies from database...");
                _logger.LogInformation("StartIndex: {}, Count: {}, Sort: {}", request.StartIndex, request.Count, string.Join(", ", request.GetSortByProperties()));

                await using var db = await _dbFactory.CreateDbContextAsync(request.CancellationToken);

                var query = db.Companies
                              .AsNoTracking()
                              .Include(c => c.Workers)
                              .Skip(request.StartIndex);

                if (request.Count.HasValue)
                {
                    query = query.Take(request.Count.Value);
                }

                var count = await db.Companies.CountAsync(request.CancellationToken);

                if (count != _totalItemsCount && !request.CancellationToken.IsCancellationRequested)
                {
                    _totalItemsCount = count;
                    StateHasChanged();
                }

                providerResult = new GridItemsProviderResult<Company>
                {
                    Items = await request.ApplySorting(query).ToArrayAsync(request.CancellationToken),
                    TotalItemCount = count
                };
            }
            catch when (request.CancellationToken.IsCancellationRequested) { }

            return providerResult;
        };
    }

    private async Task UpdateWorkerCountForAllCompanies()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        // Fetch all companies from the database
        var companies = await db.Companies.ToListAsync();

        foreach (var company in companies)
        {
            // Recalculate WorkerCount based on the number of workers assigned to each company
            company.WorkerCount = await db.Workers
                .CountAsync(w => w.AssignedCompanyId == company.CompanyId);

            // Save the updated WorkerCount to the database
            await db.SaveChangesAsync();
            _logger.LogInformation("Updated WorkerCount for company '{}': {}", company.Name, company.WorkerCount);
        }
    }

    private async Task DeleteCompanyAsync(int companyId)
    {
        _logger.LogInformation("Deleting company with id {}", companyId);

        await using var db = await _dbFactory.CreateDbContextAsync();
        var company = await db.Companies
            .Include(c => c.Workers)
            .FirstOrDefaultAsync(c => c.CompanyId == companyId);

        if (company is not null)
        {
            db.Companies.Remove(company);
            await db.SaveChangesAsync();
            _logger.LogInformation("Successfully deleted company {}", companyId);
            await _dataGrid.RefreshDataAsync();
        }
        else
        {
            _logger.LogWarning("Company with id {} not found", companyId);
        }
    }
}

