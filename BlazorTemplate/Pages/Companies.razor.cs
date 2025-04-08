using BlazorTemplate.Data;
using BlazorTemplate.Data.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlazorTemplate.Pages
{
    public partial class Companies : ComponentBase
    {
        [Inject] private IDbContextFactory<ApplicationDbContext> DbFactory { get; set; } = default!;
        [Inject] private ILogger<Companies> Logger { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!; // Inject NavigationManager

        private QuickGrid<Company> _dataGrid = default!;
        private string _searchTerm = string.Empty; // Search term for filtering
        private List<Company> _companies = new List<Company>(); // Store all companies
        private IQueryable<Company> _filteredCompanies = Enumerable.Empty<Company>().AsQueryable(); // Store filtered companies
        private int _totalItemsCount = 0; // Track total items count for display

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await LoadCompaniesAsync();
        }

        private async Task LoadCompaniesAsync()
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            _companies = await db.Companies.ToListAsync();
            _totalItemsCount = _companies.Count; // Set total item count
            _filteredCompanies = _companies.AsQueryable(); // Initially load all companies
        }

        private async Task OnSearchClick()
        {
            Logger.LogInformation("Search triggered with term: {0}", _searchTerm);

            if (string.IsNullOrEmpty(_searchTerm))
            {
                _filteredCompanies = _companies.AsQueryable(); // Show all companies if no search term
            }
            else
            {
                _filteredCompanies = _companies
                    .Where(c => c.Name.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase))
                    .AsQueryable(); // Filter companies by name
            }

            _totalItemsCount = _filteredCompanies.Count(); // Update item count based on filtered companies

            // Refresh the data grid after applying the search filter
            await _dataGrid.RefreshDataAsync();

            StateHasChanged(); // Ensure UI updates
        }

        private async Task DeleteCompanyAsync(int companyId)
        {
            Logger.LogInformation("Deleting company with id {}", companyId);

            await using var db = await DbFactory.CreateDbContextAsync();
            var company = await db.Companies
                .Include(c => c.Workers)
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);

            if (company != null)
            {
                // Nullify worker's company association
                foreach (var worker in company.Workers)
                {
                    worker.AssignedCompanyId = null;
                }

                await db.SaveChangesAsync();

                // Delete the company
                db.Companies.Remove(company);
                await db.SaveChangesAsync();

                Logger.LogInformation("Company with id {} deleted successfully.", companyId);

                // Reload the companies after deletion
                await LoadCompaniesAsync();

                // Refresh the data grid after deletion
                await _dataGrid.RefreshDataAsync();
                StateHasChanged(); // Ensure UI updates
            }
        }

        private async Task UpdateCompanyAsync(int companyId)
        {
            try
            {
                await using var db = await DbFactory.CreateDbContextAsync();

                // Fetch the company based on the provided companyId
                var company = await db.Companies.FirstOrDefaultAsync(c => c.CompanyId == companyId);

                if (company != null)
                {
                    // Redirect to the edit page with the company details preloaded
                    Navigation.NavigateTo($"/companies/edit/{companyId}");
                }
                else
                {
                    Logger.LogWarning("Company with id {CompanyId} not found", companyId);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating company.");
            }
        }
    }
}
