using Microsoft.EntityFrameworkCore;
using DossierArchiveApp.Models;

namespace DossierArchiveApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<User> Users => Set<User>();
    public DbSet<MainCategory> MainCategories => Set<MainCategory>();
    public DbSet<PrimaryCategory> PrimaryCategories => Set<PrimaryCategory>();
    public DbSet<SecondaryCategory> SecondaryCategories => Set<SecondaryCategory>();
    public DbSet<TertiaryCategory> TertiaryCategories => Set<TertiaryCategory>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentEmployee> DocumentEmployees => Set<DocumentEmployee>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
}