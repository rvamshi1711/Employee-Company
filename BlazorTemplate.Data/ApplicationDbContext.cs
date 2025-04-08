using BlazorTemplate.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorTemplate.Data;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options) {
    public const string ConnectionString = "Data Source=app.db";

    /// <summary>
    /// Represents a collection of Worker entities in the database. Allows for querying and managing worker data.
    /// </summary>
    public virtual DbSet<Worker> Workers { get; set; }

    /// <summary>
    /// Represents a collection of Company entities in the database. Provides functionality to query and save Company data.
    /// </summary>
    public virtual DbSet<Company> Companies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Company>(c =>
            c.HasData(new Company() {
                CompanyId = 1,
                Name = "Work First Casualty Company"
            },
            new Company() {
                CompanyId = 2,
                Name = "Widget Company LLC"
            },
            new Company() {
                CompanyId = 3,
                Name = "Awesome Enterprises"
            })
        );

        modelBuilder.Entity<Worker>(w =>
            w.HasData(new Worker() {
                AssignedCompanyId = 1,
                BirthDate = new DateOnly(1996, 1, 1),
                Email = "nate@email.com",
                PhoneNumber = "(555) 555-5555",
                FirstName = "Nate",
                LastName = "Tripp",
                WorkerId = 1
            })
        );
    }
}
