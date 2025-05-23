using BlazorTemplate.Data;
using BlazorTemplate.Data.Entities;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.EntityFrameworkCore;

namespace BlazorTemplate.Pages;

public partial class Home(IDbContextFactory<ApplicationDbContext> dbFactory, ILogger<Home> logger)
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory = dbFactory;
    private readonly ILogger<Home> _logger = logger;

    private QuickGrid<Worker> _dataGrid = default!;

    private PaginationState pagination = new PaginationState { ItemsPerPage = 5 };
    private GridItemsProvider<Worker> _provider = default!;
    private int _totalItemsCount;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _provider = async request =>
 {
     var providerResult = new GridItemsProviderResult<Worker>()
     {
         Items = Array.Empty<Worker>(),
         TotalItemCount = 0
     };

     try
     {
         _logger.LogInformation("Loading workers from database...");
         _logger.LogInformation("Applying filters, StartIndex: {}, Count: {}, Sorting On: {}", request.StartIndex, request.Count, string.Join(", ", request.GetSortByProperties()));

         await using var db = await _dbFactory.CreateDbContextAsync(request.CancellationToken);


         var totalCount = await db.Workers.CountAsync(request.CancellationToken);


         var workersQuery = db.Workers
             .AsNoTracking()
             .Include(c => c.AssignedCompany)
             .OrderBy(w => w.WorkerId)
             .Skip(request.StartIndex)
             .Take(request.Count ?? 0);

         providerResult = new GridItemsProviderResult<Worker>()
         {
             Items = await request.ApplySorting(workersQuery).ToArrayAsync(request.CancellationToken),
             TotalItemCount = totalCount
         };

         if (_totalItemsCount != totalCount && !request.CancellationToken.IsCancellationRequested)
         {
             _totalItemsCount = totalCount;
             StateHasChanged();
         }
     }
     catch when (request.CancellationToken.IsCancellationRequested)
     {

     }

     return providerResult;
 };

    }

    /// <summary>
    /// Deletes the provided worker from the database.
    /// WorkerCount is re-evaluated
    /// </summary>
    /// <param name="workerId">The id of the worker to delete.</param>
    private async Task DeleteWorkerAsync(int workerId)
    {
        _logger.LogInformation("Deleting worker with id {}", workerId);

        await using var db = await _dbFactory.CreateDbContextAsync();
        var worker = await db.Workers
            .Include(w => w.AssignedCompany)
            .FirstOrDefaultAsync(w => w.WorkerId == workerId);

        if (worker != null)
        {

            var assignedCompany = worker.AssignedCompany;


            var deletedCount = await db.Workers
                .Where(w => w.WorkerId == workerId)
                .ExecuteDeleteAsync();

            if (deletedCount != 0)
            {
                _logger.LogInformation("Successfully deleted worker {}", workerId);


                if (assignedCompany != null)
                {
                    assignedCompany.WorkerCount = await db.Workers
                        .Where(w => w.AssignedCompanyId == assignedCompany.CompanyId)
                        .CountAsync();


                    await db.SaveChangesAsync();
                    _logger.LogInformation("Updated WorkerCount for '{}': {}", assignedCompany.Name, assignedCompany.WorkerCount);
                }


                if (_dataGrid is not null)
                {
                    await _dataGrid.RefreshDataAsync();
                }
            }
            else
            {
                _logger.LogWarning("Worker with id {} could not be deleted. Does it exist?", workerId);
            }
        }
        else
        {
            _logger.LogWarning("Worker with id {} could not be found.", workerId);
        }
    }
}