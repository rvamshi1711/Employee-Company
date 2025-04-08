using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using BlazorTemplate.Data;

namespace InsightWorldwide.Data.Migrations.MigrationsContextFactory;
public class MigrationsContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext> {
    public ApplicationDbContext CreateDbContext(string[] args) {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(ApplicationDbContext.ConnectionString, b => b.MigrationsAssembly(GetType().Assembly.GetName().Name))
            .Options;

        return new ApplicationDbContext(options);
    }
}