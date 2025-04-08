using BlazorTemplate.Data;
using BlazorTemplate.Data.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlazorTemplate.Pages
{
    /// <summary>
    /// Component to display and manage a list of companies with search functionality.
    /// </summary>
    public partial class Companies : ComponentBase
    {
        [Inject] private IDbContextFactory<ApplicationDbContext> DbFactory { get; set; } = default!;
        [Inject] private ILogger<Companies> Logger { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;

        private QuickGrid<Company> _dataGrid = default!;
        private string _searchTerm = string.Empty;
        private List<Company> _companies = new List<Company>();
        private IQueryable<Company> _filteredCompanies = Enumerable.Empty<Company>().AsQueryable();
        private int _totalItemsCount = 0;
        /// <summary>
        /// Lifecycle method that runs when the component initializes.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await LoadCompaniesAsync();
        }
        /// <summary>
        /// Loads all companies from the database into memory.
        /// </summary>
        private async Task LoadCompaniesAsync()
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            _companies = await db.Companies.ToListAsync();
            _totalItemsCount = _companies.Count;
            _filteredCompanies = _companies.AsQueryable();
        }

        /// <summary>
        /// Filters the companies list when the search button is clicked.
        /// </summary>
        private async Task OnSearchClick()
        {
            Logger.LogInformation("Search triggered with term: {0}", _searchTerm);

            if (string.IsNullOrEmpty(_searchTerm))
            {
                _filteredCompanies = _companies.AsQueryable();
            }
            else
            {
                _filteredCompanies = _companies
                    .Where(c => c.Name.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase))
                    .AsQueryable();
            }

            _totalItemsCount = _filteredCompanies.Count();


            await _dataGrid.RefreshDataAsync();

            StateHasChanged();
        }

        /// <summary>
        /// Deletes a company by its ID, unassigns all related workers first,
        /// then reloads the company list and refreshes the grid.
        /// </summary>
        private async Task DeleteCompanyAsync(int companyId)
        {
            Logger.LogInformation("Deleting company with id {}", companyId);

            await using var db = await DbFactory.CreateDbContextAsync();
            var company = await db.Companies
                .Include(c => c.Workers)
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);

            if (company != null)
            {

                foreach (var worker in company.Workers)
                {
                    worker.AssignedCompanyId = null;
                }

                await db.SaveChangesAsync();


                db.Companies.Remove(company);
                await db.SaveChangesAsync();

                Logger.LogInformation("Company with id {} deleted successfully.", companyId);


                await LoadCompaniesAsync();


                await _dataGrid.RefreshDataAsync();
                StateHasChanged();
            }
        }

        /// <summary>
        /// Navigates to the edit page for a specific company if it exists. 
        /// </summary>
        private async Task UpdateCompanyAsync(int companyId)
        {
            try
            {
                await using var db = await DbFactory.CreateDbContextAsync();


                var company = await db.Companies.FirstOrDefaultAsync(c => c.CompanyId == companyId);

                if (company != null)
                {

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
