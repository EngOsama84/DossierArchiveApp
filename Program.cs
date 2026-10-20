using DossierArchiveApp.Components;
using DossierArchiveApp.Data;
using DossierArchiveApp.Models;
using DossierArchiveApp.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

// 1. ≈‰‘«¡ «·‹ Builder Ê ”ÃÌ· «·Œœ„«  √Ê·«
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddServerSideBlazor()
    .AddHubOptions(options =>
    {
        // —›⁄ «·Õœ «·√ﬁ’Ï ·ÕÃ„ «·—”«·… «·Ê«Õœ… «·„” ﬁ»·… ⁄»— SignalR ≈·Ï 64 „Ì€«»«Ì 
        options.MaximumReceiveMessageSize = 64 * 1024 * 1024; // 64 MB
    });

builder.Services.AddMudServices();

//  ”ÃÌ· ﬁ«⁄œ… »Ì«‰«  SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=dossiers.sqlite3"));

//  ”ÃÌ· «·Œœ„«  «·„Œ’’…
builder.Services.AddScoped<ScannerService>();
builder.Services.AddScoped<UserSessionService>();
builder.Services.AddScoped<ExportService>();

// 2. »‰«¡ «· ÿ»Ìﬁ Ê Ê·Ìœ «·„ €Ì— app (Â‰« ÌÊ·œ «·ﬂ«∆‰ app)
var app = builder.Build();

// 3.  ÂÌ∆… ﬁ«⁄œ… «·»Ì«‰«  Ê≈‰‘«¡ «·Ãœ«Ê· ÊÕ”«» «·„œÌ— (Ì√ Ì »⁄œ builder.Build)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // ≈‰‘«¡ ﬂ«›… «·Ãœ«Ê· ≈–« ·„  ﬂ‰ „ÊÃÊœ…
    db.Database.EnsureCreated();

    void TryExecuteSql(string sql)
    {
        try { db.Database.ExecuteSqlRaw(sql); } catch { }
    }

    //  —ﬁÌ«  «Õ Ì«ÿÌ… ··Ãœ«Ê·
    //TryExecuteSql("ALTER TABLE TB_SystemSettings ADD COLUMN DirectorateName TEXT DEFAULT '«·„œÌ—Ì… «·⁄«„…';");
    //TryExecuteSql("ALTER TABLE TB_SystemSettings ADD COLUMN DepartmentName TEXT DEFAULT '‘⁄»…  ﬂ‰Ê·ÊÃÌ« «·„⁄·Ê„« ';");
    //TryExecuteSql("ALTER TABLE TB_SystemSettings ADD COLUMN LogoPath TEXT DEFAULT '';");
    //TryExecuteSql("ALTER TABLE TB_SystemSettings ADD COLUMN FontFamily TEXT DEFAULT 'Dubai';");
    //TryExecuteSql("ALTER TABLE TB_SystemSettings ADD COLUMN DefaultScannerName TEXT DEFAULT '';");
    //TryExecuteSql("ALTER TABLE TB_SystemSettings ADD COLUMN ScanSource TEXT DEFAULT 'Feeder';");
    //TryExecuteSql("ALTER TABLE TB_SystemSettings ADD COLUMN DuplexScan INTEGER DEFAULT 0;");
    //TryExecuteSql("ALTER TABLE TB_SystemSettings ADD COLUMN ScannerResolutionDPI INTEGER DEFAULT 200;");
    //TryExecuteSql("ALTER TABLE TB_SystemSettings ADD COLUMN ScannerColorMode TEXT DEFAULT 'Grayscale';");
    //TryExecuteSql("ALTER TABLE TB_SystemSettings ADD COLUMN PdfCompressionLevel TEXT DEFAULT 'Medium';");

    //TryExecuteSql("ALTER TABLE TB_Documents ADD COLUMN IsDeleted INTEGER DEFAULT 0;");
    //TryExecuteSql("ALTER TABLE TB_Documents ADD COLUMN DeletedAt TEXT NULL;");
    //TryExecuteSql("ALTER TABLE TB_Employees ADD COLUMN IsDeleted INTEGER DEFAULT 0;");
    //TryExecuteSql("ALTER TABLE TB_Employees ADD COLUMN DeletedAt TEXT NULL;");

    //TryExecuteSql("ALTER TABLE TB_Users ADD COLUMN FullName TEXT DEFAULT '';");
    //TryExecuteSql("ALTER TABLE TB_Users ADD COLUMN IsAdmin INTEGER DEFAULT 0;");
    //TryExecuteSql("ALTER TABLE TB_Users ADD COLUMN CanAdd INTEGER DEFAULT 0;");
    //TryExecuteSql("ALTER TABLE TB_Users ADD COLUMN CanEdit INTEGER DEFAULT 0;");
    //TryExecuteSql("ALTER TABLE TB_Users ADD COLUMN CanDelete INTEGER DEFAULT 0;");
    //TryExecuteSql("ALTER TABLE TB_Users ADD COLUMN CanExport INTEGER DEFAULT 0;");
    //TryExecuteSql("ALTER TABLE TB_Users ADD COLUMN MustChangePassword INTEGER DEFAULT 0;");

    // --- »–— «· ’‰Ì›«  «·„” Œ—Ã… „‰ ﬁ«⁄œ… «·»Ì«‰«  «·√’·Ì… ---
    using (var seedScope = app.Services.CreateScope())
    {
        var db2 = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
        db2.Database.EnsureCreated();

        if (!db2.MainCategories.Any())
        {
            // 1. «·√‰Ê«⁄ «·—∆Ì”Ì…
            var mainIn = new MainCategory { Name = "Ê«—œ" };
            var mainOut = new MainCategory { Name = "’«œ—" };
            db2.MainCategories.AddRange(mainIn, mainOut);
            db2.SaveChanges();

            // 2. «· ’‰Ì›«  «·—∆Ì”Ì… ·ﬂ·« «·‰Ê⁄Ì‰ (Ê«—œ Ê’«œ—)
            var primaryNames = new[] { "√„— ≈œ«—Ì", "«” „«—…", "ÿ·» ‘Œ’Ì", "«⁄ Ì«œÌ…" };
            var primaryList = new List<PrimaryCategory>();

            foreach (var main in new[] { mainIn, mainOut })
            {
                foreach (var pName in primaryNames)
                {
                    primaryList.Add(new PrimaryCategory { Name = pName, MainCategoryId = main.MainCategoryId });
                }
            }
            db2.PrimaryCategories.AddRange(primaryList);
            db2.SaveChanges();

            // 3. «· ’‰Ì›«  «·›—⁄Ì… «·√Ê·Ï
            var secondaryNames = new[]
            {
            "„»«‘—…", "«‰›ﬂ«ﬂ", "⁄·«Ê…", " —›Ì⁄", "‘ﬂ—", "⁄ﬁÊ»…",
            "«Õ ”«»", "„ﬂ«›√…", " ﬂ·Ì›", " ‰”Ì»", " —‘ÌÕ", "·Ã‰…",
            " ﬁÌÌ„", "„” „”ﬂ« ", "„⁄·Ê„« ", "„Œ’’« ", " Ÿ·„",
            "‰ﬁ·", "≈Ã«“…", "≈⁄›«¡", "œÊ—…", "‰ ÌÃ…", " »·Ì€", "«” œ⁄«¡", "€Ì— –·ﬂ"
        };

            var secondaryList = new List<SecondaryCategory>();
            foreach (var prim in primaryList)
            {
                foreach (var sName in secondaryNames)
                {
                    secondaryList.Add(new SecondaryCategory { Name = sName, PrimaryCategoryId = prim.PrimaryCategoryId });
                }
            }
            db2.SecondaryCategories.AddRange(secondaryList);
            db2.SaveChanges();

            // 4. «· ’‰Ì›«  «·›—⁄Ì… «·À«‰Ì…
            var tertiaryNames = new[]
            {
            "œÊ—…", "„ƒ „—", "Ê—‘…", "≈Ã«“…", "‰ﬁ·",
            " ‰”Ì»", " ﬁ«⁄œ", " ﬂ·Ì›", "‘Â«œ…", "Œœ„…"
        };

            var tertiaryList = new List<TertiaryCategory>();
            // —»ÿÂ« »«· ’‰Ì›«  «·›—⁄Ì… «·√Ê·Ï «·„‰«”»… („À· «‰›ﬂ«ﬂ° „»«‘—…° «Õ ”«»° ≈Ã«“…° œÊ—…)
            var targetSecs = secondaryList.Where(s =>
                s.Name == "«‰›ﬂ«ﬂ" || s.Name == "„»«‘—…" || s.Name == "«Õ ”«»" ||
                s.Name == "≈Ã«“…" || s.Name == "œÊ—…" || s.Name == " ﬂ·Ì›").ToList();

            foreach (var sec in targetSecs)
            {
                foreach (var tName in tertiaryNames)
                {
                    tertiaryList.Add(new TertiaryCategory { Name = tName, SecondaryCategoryId = sec.SecondaryCategoryId });
                }
            }
            db2.TertiaryCategories.AddRange(tertiaryList);
            db2.SaveChanges();
        }
    }
    // -------------------------------------------------------------
    // «· Õﬁﬁ „‰ ÊÃÊœ Õ”«» «·„œÌ— «·«› —«÷Ì √Ê ≈‰‘«ƒÂ
    var adminUser = db.Users.FirstOrDefault(u => u.UserName.ToLower() == "admin");

    if (adminUser == null)
    {
        adminUser = new User
        {
            UserName = "admin",
            FullName = "„œÌ— «·‰Ÿ«„ «·«› —«÷Ì",
            PasswordHash = UserSessionService.HashPassword("123456"),
            IsAdmin = 1,
            CanAdd = 1,
            CanEdit = 1,
            CanDelete = 1,
            CanExport = 1,
            IsActive = 1,
            MustChangePassword = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        db.Users.Add(adminUser);
        db.SaveChanges();
    }
}

// 4. Œÿ √‰«»Ì» «·„⁄«·Ã… (Middleware Pipeline)
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

// 5.  ‘€Ì· «· ÿ»Ìﬁ
app.Run();