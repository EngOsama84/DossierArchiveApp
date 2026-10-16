using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using DossierArchiveApp.Components;
using DossierArchiveApp.Data;
using DossierArchiveApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

//  ”ÃÌ· ﬁ«⁄œ… »Ì«‰«  SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=dossiers.sqlite3"));

//  ”ÃÌ· Œœ„… «·„«”Õ «·÷Ê∆Ì
builder.Services.AddSingleton<ScannerService>();

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