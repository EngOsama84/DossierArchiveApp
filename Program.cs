using DossierArchiveApp.Components;
using DossierArchiveApp.Data;
using DossierArchiveApp.Models;
using DossierArchiveApp.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = true;
});
builder.Services.AddMudServices();

//  ”ÃÌ· ﬁ«⁄œ… »Ì«‰«  SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=dossiers.sqlite3"));

//  ”ÃÌ· Œœ„… «·„«”Õ «·÷Ê∆Ì
builder.Services.AddScoped<ScannerService>();
builder.Services.AddScoped<UserSessionService>();
builder.Services.AddScoped<ExportService>();
var app = builder.Build();

//  —ﬁÌ… „ƒﬂœ… ·ÃœÊ· «·≈⁄œ«œ«  ›Ì SQLite ⁄‰œ ﬂ· ≈ﬁ·«⁄
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    void TryAddColumn(string sql)
    {
        try { db.Database.ExecuteSqlRaw(sql); } catch { }
    }

    TryAddColumn("ALTER TABLE TB_SystemSettings ADD COLUMN DirectorateName TEXT DEFAULT '«·„œÌ—Ì… «·⁄«„…';");
    TryAddColumn("ALTER TABLE TB_SystemSettings ADD COLUMN DepartmentName TEXT DEFAULT '‘⁄»…  ﬂ‰Ê·ÊÃÌ« «·„⁄·Ê„« ';");
    TryAddColumn("ALTER TABLE TB_SystemSettings ADD COLUMN LogoPath TEXT DEFAULT '';");
    TryAddColumn("ALTER TABLE TB_SystemSettings ADD COLUMN FontFamily TEXT DEFAULT 'Dubai';");
    TryAddColumn("ALTER TABLE TB_SystemSettings ADD COLUMN DefaultScannerName TEXT DEFAULT '';");
    TryAddColumn("ALTER TABLE TB_SystemSettings ADD COLUMN ScanSource TEXT DEFAULT 'Feeder';");
    TryAddColumn("ALTER TABLE TB_SystemSettings ADD COLUMN DuplexScan INTEGER DEFAULT 0;");
    TryAddColumn("ALTER TABLE TB_SystemSettings ADD COLUMN ScannerResolutionDPI INTEGER DEFAULT 200;");
    TryAddColumn("ALTER TABLE TB_SystemSettings ADD COLUMN ScannerColorMode TEXT DEFAULT 'Grayscale';");
    TryAddColumn("ALTER TABLE TB_SystemSettings ADD COLUMN PdfCompressionLevel TEXT DEFAULT 'Medium';");

    TryAddColumn("ALTER TABLE TB_Documents ADD COLUMN IsDeleted INTEGER DEFAULT 0;");
    TryAddColumn("ALTER TABLE TB_Documents ADD COLUMN DeletedAt TEXT NULL;");
    TryAddColumn("ALTER TABLE TB_Employees ADD COLUMN IsDeleted INTEGER DEFAULT 0;");
    TryAddColumn("ALTER TABLE TB_Employees ADD COLUMN DeletedAt TEXT NULL;");
}
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // «· Õﬁﬁ „‰ ÊÃÊœ „” Œœ„ „œÌ—° Ê≈‰‘«∆Â  ·ﬁ«∆Ì« ≈–« ·„ Ìﬂ‰ „ÊÃÊœ«
    var hasAdmin = db.Users.Any(u => u.UserTypeId == 1);
    if (!hasAdmin)
    {
        var defaultAdmin = new User
        {
            UserName = "admin",
            PasswordHash = "123456", // ﬂ·„… „—Ê— «·Õ”«» «·«› —«÷Ì
            UserTypeId = 1,          // „œÌ— ‰Ÿ«„
            IsActive = 1,
            CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        db.Users.Add(defaultAdmin);
        db.SaveChanges();
    }
}
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // 1. «·»ÕÀ ⁄‰ Õ”«» admin √Ê √Ê· Õ”«» ›Ì «·ÃœÊ·
    var adminUser = db.Users.FirstOrDefault(u => u.UserName.ToLower() == "admin")
                    ?? db.Users.FirstOrDefault();

    if (adminUser != null)
    {
        //  ⁄ÌÌ‰ ﬂ·„… «·„—Ê— ≈·Ï 123456 Ê ›⁄Ì· «·Õ”«» ﬂ„œÌ— ‰Ÿ«„
        adminUser.PasswordHash = "123456";
        adminUser.UserTypeId = 1; // „œÌ— ‰Ÿ«„ ﬂ«„· «·’·«ÕÌ« 
        adminUser.IsActive = 1;   // „›⁄·
        adminUser.UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        db.SaveChanges();
    }
}
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();