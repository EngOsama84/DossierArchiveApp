using System.Diagnostics;
using System.Runtime.InteropServices;
using DossierArchiveApp.Components;
using DossierArchiveApp.Data;
using DossierArchiveApp.Models;
using DossierArchiveApp.Services;
using Microsoft.AspNetCore.Components.Server.Circuits;
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
    })
    .AddCircuitOptions(options =>
    {
        // ÊŞáíÕ ãåáÉ ÇáÇÍÊİÇÙ ÈÇáÏÇÆÑÉ ÈÚÏ ÅÛáÇŞ ÇáäÇİĞÉ ãä 3 ÏŞÇÆŞ Åáì ËÇäíÉ æÇÍÏÉ İŞØ
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromSeconds(1);
    });

builder.Services.AddMudServices();

// ÊÓÌíá ÇáÎÏãÇÊ ÇáãÎÕÕÉ ßÜ Scoped
builder.Services.AddScoped<ScannerService>();
builder.Services.AddScoped<UserSessionService>();
builder.Services.AddScoped<ExportService>();

// ÊÓÌíá ãÑÇŞÈ ÇáÇÊÕÇá áÅÛáÇŞ ÇáÓíÑİÑ İæÑ ÅÛáÇŞ äÇİĞÉ ÇáÈÑäÇãÌ (ÒÑ X)
builder.Services.AddScoped<CircuitHandler, DesktopCircuitHandler>();

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

// ÊÚØíá ÇáÊæÌíå áÜ HTTPS áßæä ÇáÊØÈíŞ íÚãá ãÍáíÇğ/ÃæİáÇíä ÈÏæä ÔåÇÏÉ SSL
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ==================== ÈÏÇíÉ ßæÏ ÇáÊÔÛíá ÇáãßÊÈí ÇáĞßí ====================
const string appUrl = "http://127.0.0.1:5055";

// 1. ãäÚ ÊÔÛíá ÃßËÑ ãä ÓíÑİÑ İí ÇáÎáİíÉ ÚäÏ ÊßÑÇÑ ÇáäŞÑ Úáì ÇáÃíŞæäÉ
using var mutex = new System.Threading.Mutex(true, "DossierArchiveApp_SingleInstanceMutex", out bool isNewInstance);

if (!isNewInstance)
{
    // ÇáÓíÑİÑ íÚãá ÈÇáİÚá İí ÇáÎáİíÉº äİÊÍ æÇÌåÉ ÇáÈÑäÇãÌ İæÑÇğ æäÛáŞ ÇáÚãáíÉ ÇáãßÑÑÉ
    LaunchDesktopWindow(appUrl, null);
    return;
}

// 2. ÖÈØ ÚäÇæíä ÇáÇÓÊãÇÚ ÈãÇ íÖãä ÇáÊæÇİŞ ãÚ ÈíÆÉ ÇáÊØæíÑ æÇáÅäÊÇÌ
if (!app.Environment.IsDevelopment())
{
    app.Urls.Clear();
    app.Urls.Add(appUrl);
}
else
{
    if (!app.Urls.Contains(appUrl))
    {
        app.Urls.Add(appUrl);
    }
}

// 3. ÊÔÛíá ÇáÓíÑİÑ æÇáÇäÊÙÇÑ ÍÊì íÕÈÍ ÌÇåÒÇğ ÊãÇãÇğ áÇÓÊŞÈÇá ÇáØáÈÇÊ
await app.StartAsync();

// 4. İÊÍ äÇİĞÉ ÇáÈÑäÇãÌ ÇáãßÊÈíÉ ÇáãÓÊŞáÉ ÈãáÁ ÇáÔÇÔÉ æãÑÇŞÈÉ ÅÛáÇŞåÇ
LaunchDesktopWindow(appUrl, app.Lifetime);

// 5. ÅÈŞÇÁ ÇáÓíÑİÑ íÚãá ÍÊì íÛáŞ ÇáãÓÊÎÏã ÇáäÇİĞÉ
await app.WaitForShutdownAsync();

// ==========================================
// ÇáÏæÇá ÇáãÓÇÚÏÉ ááÊÔÛíá æÊßÈíÑ ÇáäÇİĞÉ
// ==========================================
static void LaunchDesktopWindow(string url, IHostApplicationLifetime? lifetime)
{
    string? browserPath = FindChromiumBrowserPath();

    if (!string.IsNullOrEmpty(browserPath))
    {
        try
        {
            // ãÓÇÑ ãÓÊŞá æÌÏíÏ áÖãÇä ÚÏã ŞÑÇÁÉ ÇáÍÌã ÇáÕÛíÑ ÇáÓÇÈŞ æáÚÒá ÚãáíÉ ÇáäÇİĞÉ
            string userDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DossierArchiveApp_DesktopUI");

            Directory.CreateDirectory(userDataDir);

            // ÅäÔÇÁ ãáİ First Run İÇÑÛ áãäÚ ÙåæÑ äÇİĞÉ ÇáÊÑÍíÈ æÇáãÒÇãäÉ İí Edge æ Chrome
            string firstRunFile = Path.Combine(userDataDir, "First Run");
            if (!File.Exists(firstRunFile))
            {
                File.WriteAllText(firstRunFile, string.Empty);
            }

            var startTime = DateTime.UtcNow;

            var uiProcess = Process.Start(new ProcessStartInfo
            {
                FileName = browserPath,
                Arguments = $"--app={url} --start-maximized --user-data-dir=\"{userDataDir}\" --no-first-run --no-default-browser-check --disable-sync --disable-extensions --disable-features=Translate,msImplicitSignin,msEdgeFre",
                UseShellExecute = false
            });

            if (uiProcess != null)
            {
                _ = Task.Run(async () =>
                {
                    // ÅÌÈÇÑ äÙÇã æíäÏæÒ Úáì ÊßÈíÑ ÇáäÇİĞÉ (Maximize) İæÑ ÙåæÑåÇ Úáì ÇáÔÇÔÉ
                    for (int i = 0; i < 25; i++)
                    {
                        if (uiProcess.HasExited) break;
                        uiProcess.Refresh();
                        if (uiProcess.MainWindowHandle != IntPtr.Zero)
                        {
                            NativeWindowHelper.MaximizeWindow(uiProcess.MainWindowHandle);
                            break;
                        }
                        await Task.Delay(200);
                    }

                    // ÇäÊÙÇÑ ÅÛáÇŞ ÇáäÇİĞÉ ãä ŞÈá ÇáãÓÊÎÏã (ÒÑ X)
                    await uiProcess.WaitForExitAsync();

                    // ÅĞÇ ÇÓÊãÑÊ ÇáÚãáíÉ áÃßËÑ ãä ËÇäíÊíä Ëã ÃõÛáŞÊ¡ äæŞİ ÇáÓíÑİÑ İæÑÇğ
                    if (lifetime != null && (DateTime.UtcNow - startTime).TotalSeconds > 2)
                    {
                        lifetime.StopApplication();
                        await Task.Delay(300);
                        Environment.Exit(0);
                    }
                });
                return;
            }
        }
        catch
        {
            // İí ÍÇá ÍÏæË Ãí ÇÓÊËäÇÁ íäÊŞá ááãÊÕİÍ ÇáÇİÊÑÇÖí ÈÇáÃÓİá
        }
    }

    // ÇáÎíÇÑ ÇáÈÏíá ÇáÂãä: ÇáİÊÍ ÚÈÑ ÇáãÊÕİÍ ÇáÇİÊÑÇÖí ááäÙÇã
    Process.Start(new ProcessStartInfo
    {
        FileName = url,
        UseShellExecute = true
    });
}

static string? FindChromiumBrowserPath()
{
    string[] candidatePaths =
    [
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Browser", "chrome.exe"),
        @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
        @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
        @"C:\Program Files\Google\Chrome\Application\chrome.exe",
        @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\Application\chrome.exe")
    ];

    foreach (var path in candidatePaths)
    {
        if (File.Exists(path))
            return path;
    }

    return null;
}

// ==========================================
// ÇáÏæÇá ÇáãÓÇÚÏÉ áÊÃÓíÓ æÈĞÑ ÇáÈíÇäÇÊ ÇáÃæáíÉ
// ==========================================
static async Task SeedInitialDataAsync(AppDbContext db)
{
    // 1. ÈĞÑ ÔÌÑÉ ÇáÊÕäíİÇÊ ÇáÇİÊÑÇÖíÉ
    if (!await db.MainCategories.AnyAsync())
    {
        var mainIn = new MainCategory { Name = "æÇÑÏ" };
        var mainOut = new MainCategory { Name = "ÕÇÏÑ" };
        await db.MainCategories.AddRangeAsync(mainIn, mainOut);
        await db.SaveChangesAsync();

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

// =========================================================================
// ÇáİÆÇÊ ÇáãÓÇÚÏÉ ááÊßÈíÑ ÇáÊáŞÇÆí æãÑÇŞÈÉ ÇáÅÛáÇŞ ÇáİæÑí (İí äåÇíÉ Çáãáİ)
// =========================================================================
public static class NativeWindowHelper
{
    private const int SW_MAXIMIZE = 3;

    [DllImport("user32.dll")]
    private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    public static void MaximizeWindow(IntPtr hWnd)
    {
        if (hWnd != IntPtr.Zero)
        {
            ShowWindowAsync(hWnd, SW_MAXIMIZE);
            SetForegroundWindow(hWnd);
        }
    }
}

public class DesktopCircuitHandler : CircuitHandler
{
    private static int _activeConnections = 0;
    private static readonly object _lock = new();
    private static CancellationTokenSource? _shutdownCts;
    private readonly IHostApplicationLifetime _lifetime;

    public DesktopCircuitHandler(IHostApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            _activeConnections++;
            _shutdownCts?.Cancel();
            _shutdownCts = null;
        }
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            _activeConnections--;
            if (_activeConnections <= 0)
            {
                _activeConnections = 0;
                _shutdownCts?.Cancel();
                _shutdownCts = new CancellationTokenSource();
                var token = _shutdownCts.Token;

                // ÇäÊÙÇÑ 1.5 ËÇäíÉ İŞØ ááÊÃßÏ ãä ÃäåÇ áíÓÊ ÅÚÇÏÉ ÊÍãíá (Refresh) Ëã ÅÛáÇŞ ÇáÈÑäÇãÌ ÈÇáßÇãá
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(1500, token);
                        if (!token.IsCancellationRequested)
                        {
                            _lifetime.StopApplication();
                            await Task.Delay(300);
                            Environment.Exit(0);
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        // Êã ÅáÛÇÁ ÇáÅÛáÇŞ áÃä ÇáãÓÊÎÏã ŞÇã ÈÚãá Refresh ááÕİÍÉ
                    }
                });
            }
        }
        return Task.CompletedTask;
    }
}