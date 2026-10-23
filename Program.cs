using DossierArchiveApp.Components;
using DossierArchiveApp.Data;
using DossierArchiveApp.Models;
using DossierArchiveApp.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

// 1. ÅäÔÇÁ ÇáÜ Builder æÊÓÌíá ÇáÎÏãÇÊ
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddServerSideBlazor()
    .AddHubOptions(options =>
    {
        // ÑİÚ ÇáÍÏ ÇáÃŞÕì áÍÌã ÇáÑÓÇáÉ ÇáæÇÍÏÉ ÇáãÓÊŞÈáÉ ÚÈÑ SignalR Åáì 64 ãíÛÇÈÇíÊ
        options.MaximumReceiveMessageSize = 64 * 1024 * 1024; // 64 MB
    });

builder.Services.AddMudServices();

// ÊÓÌíá ÇáÎÏãÇÊ ÇáãÎÕÕÉ ßÜ Scoped
builder.Services.AddScoped<ScannerService>();
builder.Services.AddScoped<UserSessionService>();
builder.Services.AddScoped<ExportService>();

// ŞÑÇÁÉ ãÓÇÑ ÇáÇÊÕÇá ãä appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Data Source=dossiers.sqlite3";

// ÊÓÌíá ÇáÜ Factory ßÜ Singleton ãÓÊŞá æäÙíİ
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// ÊÓÌíá AppDbContext ßÜ Scoped ãÚÒæá æÈÓíØ ãÓÊÎÑÌ ãä ÇáÜ Factory ãÈÇÔÑÉ
builder.Services.AddScoped<AppDbContext>(sp =>
    sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

// 2. ÈäÇÁ ÇáÊØÈíŞ
var app = builder.Build();

// 3. ÊåíÆÉ ŞÇÚÏÉ ÇáÈíÇäÇÊ æÅäÔÇÁ ÇáÌÏÇæá æÍÓÇÈ ÇáãÏíÑ ÈÇáÇÚÊãÇÏ ÇáßÇãá Úáì Async
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // ÊØÈíŞ ÇáÜ Migrations æÈäÇÁ ÇáÌÏÇæá æÇáİåÇÑÓ İí ÍÇá áã Êßä ãæÌæÏÉ
    await db.Database.MigrateAsync();

    // ÈĞÑ ÇáÈíÇäÇÊ ÇáÊÃÓíÓíÉ
    await SeedInitialDataAsync(db);
}

// 4. ÎØ ÃäÇÈíÈ ÇáãÚÇáÌÉ (Middleware Pipeline)
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

// 5. ÊÔÛíá ÇáÊØÈíŞ
await app.RunAsync();


// ==========================================
// ÇáÏæÇá ÇáãÓÇÚÏÉ áÊÃÓíÓ æÈĞÑ ÇáÈíÇäÇÊ ÇáÃæáíÉ
// ==========================================
static async Task SeedInitialDataAsync(AppDbContext db)
{
    // 1. ÈĞÑ ÔÌÑÉ ÇáÊÕäíİÇÊ ÇáÇİÊÑÇÖíÉ
    if (!await db.MainCategories.AnyAsync())
    {
        // ÇáÃäæÇÚ ÇáÑÆíÓíÉ
        var mainIn = new MainCategory { Name = "æÇÑÏ" };
        var mainOut = new MainCategory { Name = "ÕÇÏÑ" };
        await db.MainCategories.AddRangeAsync(mainIn, mainOut);
        await db.SaveChangesAsync();

        // ÇáÊÕäíİÇÊ ÇáÑÆíÓíÉ áßáÇ ÇáäæÚíä (æÇÑÏ æÕÇÏÑ)
        var primaryNames = new[] { "ÃãÑ ÅÏÇÑí", "ÇÓÊãÇÑÉ", "ØáÈ ÔÎÕí", "ÇÚÊíÇÏíÉ" };
        var primaryList = new List<PrimaryCategory>();

        foreach (var main in new[] { mainIn, mainOut })
        {
            foreach (var pName in primaryNames)
            {
                primaryList.Add(new PrimaryCategory
                {
                    Name = pName,
                    MainCategoryId = main.MainCategoryId
                });
            }
        }
        await db.PrimaryCategories.AddRangeAsync(primaryList);
        await db.SaveChangesAsync();

        // ÇáÊÕäíİÇÊ ÇáİÑÚíÉ ÇáÃæáì
        var secondaryNames = new[]
        {
            "ãÈÇÔÑÉ", "ÇäİßÇß", "ÚáÇæÉ", "ÊÑİíÚ", "ÔßÑ", "ÚŞæÈÉ",
            "ÇÍÊÓÇÈ", "ãßÇİÃÉ", "Êßáíİ", "ÊäÓíÈ", "ÊÑÔíÍ", "áÌäÉ",
            "ÊŞííã", "ãÓÊãÓßÇÊ", "ãÚáæãÇÊ", "ãÎÕÕÇÊ", "ÊÙáã",
            "äŞá", "ÅÌÇÒÉ", "ÅÚİÇÁ", "ÏæÑÉ", "äÊíÌÉ", "ÊÈáíÛ", "ÇÓÊÏÚÇÁ", "ÛíÑ Ğáß"
        };

        var secondaryList = new List<SecondaryCategory>();
        foreach (var prim in primaryList)
        {
            foreach (var sName in secondaryNames)
            {
                secondaryList.Add(new SecondaryCategory
                {
                    Name = sName,
                    PrimaryCategoryId = prim.PrimaryCategoryId
                });
            }
        }
        await db.SecondaryCategories.AddRangeAsync(secondaryList);
        await db.SaveChangesAsync();

        // ÇáÊÕäíİÇÊ ÇáİÑÚíÉ ÇáËÇäíÉ
        var tertiaryNames = new[]
        {
            "ÏæÑÉ", "ãÄÊãÑ", "æÑÔÉ", "ÅÌÇÒÉ", "äŞá",
            "ÊäÓíÈ", "ÊŞÇÚÏ", "Êßáíİ", "ÔåÇÏÉ", "ÎÏãÉ"
        };

        var tertiaryList = new List<TertiaryCategory>();
        var targetSecs = secondaryList.Where(s =>
            s.Name == "ÇäİßÇß" || s.Name == "ãÈÇÔÑÉ" || s.Name == "ÇÍÊÓÇÈ" ||
            s.Name == "ÅÌÇÒÉ" || s.Name == "ÏæÑÉ" || s.Name == "Êßáíİ").ToList();

        foreach (var sec in targetSecs)
        {
            foreach (var tName in tertiaryNames)
            {
                tertiaryList.Add(new TertiaryCategory
                {
                    Name = tName,
                    SecondaryCategoryId = sec.SecondaryCategoryId
                });
            }
        }
        await db.TertiaryCategories.AddRangeAsync(tertiaryList);
        await db.SaveChangesAsync();
    }

    // 2. ÈĞÑ ÍÓÇÈ ÇáãÏíÑ ÇáÇİÊÑÇÖí
    var adminUser = await db.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == "admin");
    if (adminUser == null)
    {
        adminUser = new User
        {
            UserName = "admin",
            FullName = "ãÏíÑ ÇáäÙÇã ÇáÇİÊÑÇÖí",
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

        await db.Users.AddAsync(adminUser);
        await db.SaveChangesAsync();
    }

    // 3. ÈĞÑ ÅÚÏÇÏÇÊ ÇáäÙÇã æÇáÔÚÇÑ æÇáãÓÇÑ ÇáÇİÊÑÇÖí
    var setting = await db.SystemSettings.FirstOrDefaultAsync();
    if (setting == null)
    {
        setting = new SystemSetting
        {
            SettingId = 1,
            DirectorateName = "ÏíæÇä ÇáæŞİ ÇáÔíÚí",
            DepartmentName = "ÔÚÈÉ ÊßäæáæÌíÇ ÇáãÚáæãÇÊ",
            BaseFolderPath = @"D:\ÇáÇÖÈÇÑÉ ÇáÇáßÊÑæäíÉ ááãæÙİíä",
            LogoPath = "images/default_logo.png",
            FontFamily = "Dubai",
            PDFViewerMode = "internal",
            DefaultScannerName = string.Empty,
            ScanSource = "Feeder",
            DuplexScan = 0,
            ScannerResolutionDPI = 200,
            ScannerColorMode = "Grayscale",
            PdfCompressionLevel = "Medium",
            EmployeeDisplayMode = "Active",
            DefaultLanguage = "ar",
            BackupFolderPath = string.Empty,
            AutoBackupEnabled = 0,
            AutoBackupInterval = 24,
            MaxFileSize = 52428800,
            DefaultSearchLimit = 200,
            KeepAuditLog = 1
        };

        await db.SystemSettings.AddAsync(setting);
        await db.SaveChangesAsync();
    }

    // 4. ÈĞÑ ÇáÔõÚÈ æÇáæÍÏÇÊ ÇáÅÏÇÑíÉ ÇáÊÇÈÚÉ æÇáãÓÊŞáÉ
    if (!await db.Departments.AnyAsync())
    {
        var departments = new List<Department>
        {
            new Department
            {
                Name = "ÇáÔÚÈÉ ÇáÅÏÇÑíÉ æÇáãÇáíÉ",
                SubUnits = new List<SubUnit>
                {
                    new SubUnit { Name = "æÍÏÉ ÅÏÇÑÉ ÇáãæÇÑÏ ÇáÈÔÑíÉ" },
                    new SubUnit { Name = "æÍÏÉ ÇáÊÏÑíÈ æÇáÊØæíÑ" },
                    new SubUnit { Name = "æÍÏÉ ÇáÍÓÇÈÇÊ" },
                    new SubUnit { Name = "æÍÏÉ ÇáÎÏãÇÊ ÇáÅÏÇÑíÉ" },
                    new SubUnit { Name = "æÍÏÉ ÇáãÎÇÒä" }
                }
            },
            new Department
            {
                Name = "ÔÚÈÉ ÔÄæä ÇáãæÇØäíä",
                SubUnits = new List<SubUnit>
                {
                    new SubUnit { Name = "æÍÏÉ Ğæí ÇáÇÍÊíÇÌÇÊ ÇáÎÇÕÉ" }
                }
            },
            new Department { Name = "ÇáÔÚÈÉ ÇáŞÇäæäíÉ" },
            new Department
            {
                Name = "ÔÚÈÉ ÇáÃãáÇß ÇáãæŞæİÉ",
                SubUnits = new List<SubUnit>
                {
                    new SubUnit { Name = "æÍÏÉ ÍÓÇÈÇÊ ÇáÃãáÇß ÇáãæŞæİÉ" },
                    new SubUnit { Name = "æÍÏÉ ÇáÒÑÇÚí" },
                    new SubUnit { Name = "æÍÏÉ ÇáÊÌÇÑí" },
                    new SubUnit { Name = "æÍÏÉ ÇáÓßäí" },
                    new SubUnit { Name = "æÍÏÉ ÇáÚŞæÏ" }
                }
            },
            new Department
            {
                Name = "ÔÚÈÉ ÇáÊÎØíØ æÇáãÊÇÈÚÉ",
                SubUnits = new List<SubUnit>
                {
                    new SubUnit { Name = "æÍÏÉ ÇáÓáÇãÉ ÇáãåäíÉ" }
                }
            },
            new Department
            {
                Name = "ÇáÔÚÈÉ ÇáåäÏÓíÉ",
                SubUnits = new List<SubUnit>
                {
                    new SubUnit { Name = "æÍÏÉ ÇáãÓÇÍÉ" },
                    new SubUnit { Name = "æÍÏÉ ÇáßåÑæãíßÇäíß" }
                }
            },
            new Department
            {
                Name = "ÔÚÈÉ ÅÍíÇÁ ÇáÔÚÇÆÑ ÇáÍÓíäíÉ",
                SubUnits = new List<SubUnit>
                {
                    new SubUnit { Name = "æÍÏÉ ÊÓÌíá ÇáãæÇßÈ ÇáÍÓíäíÉ" }
                }
            },
            new Department { Name = "ÔÚÈÉ ÇáÅÚáÇã æÇáÇÊÕÇá ÇáÍßæãí" },
            new Department { Name = "ÔÚÈÉ ÇáÈÍæË æÇáÏÑÇÓÇÊ" },
            new Department { Name = "ÔÚÈÉ ÇáÊÏŞíŞ æÇáÑŞÇÈÉ ÇáÏÇÎáíÉ" },
            new Department
            {
                Name = "ÔÚÈÉ ÇáÊÚáíã ÇáÏíäí æÇáÏÑÇÓÇÊ ÇáÅÓáÇãíÉ",
                SubUnits = new List<SubUnit>
                {
                    new SubUnit { Name = "æÍÏÉ ÇáãæÇÑÏ ÇáÈÔÑíÉ" },
                    new SubUnit { Name = "æÍÏÉ ÇáÊÚáíã ÇáÇÓáÇãí" },
                    new SubUnit { Name = "æÍÏÉ ÇáÇÚÏÇÏíÇÊ æÇáãÚÇåÏ ÇáãáÛÇÉ" },
                    new SubUnit { Name = "æÍÏÉ ÇáÇãÊÍÇä ÇáÎÇÑÌí" },
                    new SubUnit { Name = "æÍÏÉ ÇáÊÚáíã ÇáÚÇã" }
                }
            },
            new Department
            {
                Name = "ÔÚÈÉ ÇáãÄÓÓÇÊ ÇáÏíäíÉ æÇáÎíÑíÉ",
                SubUnits = new List<SubUnit>
                {
                    new SubUnit { Name = "æÍÏÉ ÇáãæÇÑÏ ÇáÈÔÑíÉ" },
                    new SubUnit { Name = "æÍÏÉ ÇáÍÌÌ æÇáÊæáíÇÊ" },
                    new SubUnit { Name = "æÍÏÉ ÇáãÓÇÌÏ" },
                    new SubUnit { Name = "æÍÏÉ ÇáãŞÇãÇÊ" }
                }
            },
            new Department { Name = "ÔÚÈÉ ÊßäæáæÌíÇ ÇáãÚáæãÇÊ" },
            new Department { Name = "ÔÚÈÉ ãÊÇÈÚÉ ÇáÇÓÊËãÇÑ" }
        };

        await db.Departments.AddRangeAsync(departments);

        // ÅÖÇİÉ ÇáæÍÏÇÊ ÇáãÓÊŞáÉ
        var independentUnits = new List<SubUnit>
        {
            new SubUnit { Name = "æÍÏÉ ÇáÊÕÇÑíÍ ÇáÃãäíÉ æÇáãÚáæãÇÊ", DepartmentId = null },
            new SubUnit { Name = "æÍÏÉ ÓáÇãÉ ÇááÛÉ ÇáÚÑÈíÉ", DepartmentId = null },
            new SubUnit { Name = "æÍÏÉ ÇáÏİÇÚ ÇáãÏäí", DepartmentId = null },
            new SubUnit { Name = "æÍÏÉ ÔÄæä ÇáãÑÃÉ", DepartmentId = null },
            new SubUnit { Name = "æÍÏÉ Úáæã ÇáŞÑÂä", DepartmentId = null }
        };

        await db.SubUnits.AddRangeAsync(independentUnits);
        await db.SaveChangesAsync();
    }
}