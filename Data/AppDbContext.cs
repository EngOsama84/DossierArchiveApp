using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. تصفية السجلات غير المحذوفة تلقائياً على مستوى كامل النظام
        modelBuilder.Entity<Document>().HasQueryFilter(d => d.IsDeleted == 0);
        modelBuilder.Entity<Employee>().HasQueryFilter(e => e.IsDeleted == 0);

        // 2. محوّل قياسي يعتمد دوال مساعدة ساكنة لتجنب قيود Expression Trees مع معلمات out
        var dateTimeConverter = new ValueConverter<DateTime, string>(
            v => v.ToString("yyyy-MM-dd HH:mm:ss"),
            v => ParseDateTimeSafe(v)
        );

        var nullableDateTimeConverter = new ValueConverter<DateTime?, string>(
            v => v.HasValue ? v.Value.ToString("yyyy-MM-dd HH:mm:ss") : null!,
            v => ParseNullableDateTimeSafe(v)
        );

        // 3. تطبيق المحول تلقائياً على جميع خصائص التواريخ في كافة جداول النظام
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableDateTimeConverter);
                }
            }
        }
    }

    private static DateTime ParseDateTimeSafe(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DateTime.MinValue;

        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
            ? dt
            : DateTime.Parse(value);
    }

    private static DateTime? ParseNullableDateTimeSafe(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
            ? dt
            : null;
    }
}