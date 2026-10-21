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

        // 2. فهارس تحسين الأداء وسرعة الاستعلامات (Indexes)
        // أ. جدول الموظفين
        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.FileNumber)
            .HasDatabaseName("IX_Employees_FileNumber");

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.EmployeeName)
            .HasDatabaseName("IX_Employees_EmployeeName");

        // ب. جدول المستندات
        modelBuilder.Entity<Document>()
            .HasIndex(d => d.DocumentNumber)
            .HasDatabaseName("IX_Documents_DocumentNumber");

        modelBuilder.Entity<Document>()
            .HasIndex(d => d.DocumentDate)
            .HasDatabaseName("IX_Documents_DocumentDate");

        modelBuilder.Entity<Document>()
            .HasIndex(d => new { d.MainCategoryId, d.PrimaryCategoryId })
            .HasDatabaseName("IX_Documents_Categories");

        // ج. جدول ربط المستندات بالموظفين (فهرس فائق الأهمية للاستعلامات والـ Join)
        modelBuilder.Entity<DocumentEmployee>()
            .HasIndex(de => new { de.EmployeeId, de.DocumentId })
            .HasDatabaseName("IX_DocumentEmployees_Employee_Document");

        modelBuilder.Entity<DocumentEmployee>()
            .HasIndex(de => de.DocumentId)
            .HasDatabaseName("IX_DocumentEmployees_DocumentId");

        // د. جدول سجل النشاطات (لتسريع تقرير المراقبة والطباعة)
        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.CreatedAt)
            .HasDatabaseName("IX_AuditLogs_CreatedAt");

        // 3. محوّل قياسي يعتمد دوال مساعدة ساكنة لتجنب قيود Expression Trees مع معلمات out
        var dateTimeConverter = new ValueConverter<DateTime, string>(
            v => v.ToString("yyyy-MM-dd HH:mm:ss"),
            v => ParseDateTimeSafe(v)
        );

        var nullableDateTimeConverter = new ValueConverter<DateTime?, string>(
            v => v.HasValue ? v.Value.ToString("yyyy-MM-dd HH:mm:ss") : null!,
            v => ParseNullableDateTimeSafe(v)
        );

        // 4. تطبيق المحول تلقائياً على جميع خصائص التواريخ في كافة جداول النظام
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