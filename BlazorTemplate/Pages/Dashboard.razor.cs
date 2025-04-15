using BlazorTemplate.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlazorTemplate.Pages
{
    /// <summary>
    /// Dashboard component to display a bar chart showing the number of employees per company.
    /// </summary>
    public partial class Dashboard : ComponentBase
    {
        [Inject] private IDbContextFactory<ApplicationDbContext> DbFactory { get; set; } = default!;
        [Inject] private ILogger<Dashboard> Logger { get; set; } = default!;

        protected List<ChartModel> chartData = new();
        private bool chartDataLoaded = false;

        private int totalCompanies;
        private int totalEmployees;

        public class ChartModel
        {
            public string CompanyName { get; set; } = string.Empty;
            public int EmployeeCount { get; set; }
        }


        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await LoadChartDataAsync();
        }


        private async Task LoadChartDataAsync()
        {
            try
            {
                await using var db = await DbFactory.CreateDbContextAsync();

                totalCompanies = await db.Companies.CountAsync();
                totalEmployees = await db.Workers.CountAsync();

                chartData = await db.Companies
                    .Include(c => c.Workers)
                    .Select(c => new ChartModel
                    {
                        CompanyName = c.Name,
                        EmployeeCount = c.Workers.Count
                    })
                    .ToListAsync();

                chartDataLoaded = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading chart data in Dashboard.razor");
            }
        }
    }
}
