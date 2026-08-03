using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WatchListScreening.Domain.Entities;

namespace WatchListScreening.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<SanctionEntry>  SanctionEntries => Set<SanctionEntry>();
    public DbSet<ScreeningRequest> ScreeningRequests => Set<ScreeningRequest>();
    public DbSet<ScreeningResult> ScreeningResults => Set<ScreeningResult>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
