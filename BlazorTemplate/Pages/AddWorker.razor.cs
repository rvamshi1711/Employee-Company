using BlazorTemplate.Data;
using BlazorTemplate.Data.Entities;
using BlazorTemplate.Ext;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorTemplate.Pages;

public partial class AddWorker(IDbContextFactory<ApplicationDbContext> dbFactory, ILogger<AddWorker> logger, NavigationManager navManager) {
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory = dbFactory;
    private readonly ILogger<AddWorker> _logger = logger;
    private readonly Model _model = new();
    private readonly NavigationManager _navManager = navManager;

    private Company[] _companies = default!;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync() {
        await base.OnInitializedAsync();
        await using var db = await _dbFactory.CreateDbContextAsync();
        _companies = await db.Companies
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToArrayAsync();
        _logger.LogInformation("Discovered {} companies available", _companies.Length);
    }

    /// <summary>
    /// Adds the entered worker into the database and returns the user to the workers page.
    /// </summary>
    private async Task SubmitWorkerAsync() {
        _logger.LogInformation("Saving new worker {}", _model.FirstName + " " + _model.LastName);
        var worker = new Worker {
            AssignedCompanyId = _model.AssignedCompanyId,
            FirstName = _model.FirstName,
            LastName = _model.LastName,
            Email = _model.Email,
            PhoneNumber = _model.PhoneNumber.FormatAsPhoneNumber(),
            BirthDate = _model.BirthDate!.Value
        };

        await using var db = await _dbFactory.CreateDbContextAsync();
        await db.AddAsync(worker);
        await db.SaveChangesAsync();
        _navManager.NavigateTo("/");
    }

    private sealed class Model {
        public int? AssignedCompanyId { get; set; }

        [Required, Range(typeof(DateOnly), "01/01/1920", "01/01/2020", ErrorMessage = "Value for {0} must be between {1} and {2}")]
        public DateOnly? BirthDate { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string FirstName { get; set; } = default!;

        [Required]
        public string LastName { get; set; } = default!;

        [Required, Phone, MinLength(10), MaxLength(14)]
        public string PhoneNumber { get; set; } = default!;
    }
}