using DossierArchiveApp.Components;
using DossierArchiveApp.Data;
using DossierArchiveApp.Models;
using DossierArchiveApp.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

// 1. ≈‰‘«¡ «·‹ Builder Ê ”ÃÌ· «·Œœ„« 
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

// ﬁ—«¡… „”«— «·« ’«· „‰ appsettings.json „⁄ ﬁÌ„… «Õ Ì«ÿÌ… «› —«÷Ì… ›Ì Õ«· ⁄œ„ «·⁄ÀÊ— ⁄·ÌÂ
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Data Source=dossiers.sqlite3";

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// ≈ «Õ… Õﬁ‰ AppDbContext «·„»«‘— (@inject AppDbContext) ·÷„«‰ «· Ê«›ﬁ «· «„ „⁄ ﬂ«›… «·’›Õ«  «·Õ«·Ì…
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

//  ”ÃÌ· «·Œœ„«  «·„Œ’’…
builder.Services.AddScoped<ScannerService>();
builder.Services.AddScoped<UserSessionService>();
builder.Services.AddScoped<ExportService>();

// 2. »‰«¡ «· ÿ»Ìﬁ
var app = builder.Build();

// 3.  ÂÌ∆… ﬁ«⁄œ… «·»Ì«‰«  Ê≈‰‘«¡ «·Ãœ«Ê· ÊÕ”«» «·„œÌ— »«·«⁄ „«œ «·ﬂ«„· ⁄·Ï Async
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    //  ÿ»Ìﬁ «·‹ Migrations Ê»‰«¡ «·Ãœ«Ê· Ê«·›Â«—” ›Ì Õ«· ·„  ﬂ‰ „ÊÃÊœ…
    await db.Database.MigrateAsync();

    // »–— «·»Ì«‰«  «· √”Ì”Ì…
    await SeedInitialDataAsync(db);
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
await app.RunAsync();


// ==========================================
// «·œÊ«· «·„”«⁄œ… · √”Ì” Ê»–— «·»Ì«‰«  «·√Ê·Ì…
// ==========================================
static async Task SeedInitialDataAsync(AppDbContext db)
{
    // »–— ‘Ã—… «· ’‰Ì›«  «·«› —«÷Ì…
    if (!await db.MainCategories.AnyAsync())
    {
        // 1. «·√‰Ê«⁄ «·—∆Ì”Ì…
        var mainIn = new MainCategory { Name = "Ê«—œ" };
        var mainOut = new MainCategory { Name = "’«œ—" };
        await db.MainCategories.AddRangeAsync(mainIn, mainOut);
        await db.SaveChangesAsync();

        // 2. «· ’‰Ì›«  «·—∆Ì”Ì… ·ﬂ·« «·‰Ê⁄Ì‰ (Ê«—œ Ê’«œ—)
        var primaryNames = new[] { "√„— ≈œ«—Ì", "«” „«—…", "ÿ·» ‘Œ’Ì", "«⁄ Ì«œÌ…" };
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
                secondaryList.Add(new SecondaryCategory
                {
                    Name = sName,
                    PrimaryCategoryId = prim.PrimaryCategoryId
                });
            }
        }
        await db.SecondaryCategories.AddRangeAsync(secondaryList);
        await db.SaveChangesAsync();

        // 4. «· ’‰Ì›«  «·›—⁄Ì… «·À«‰Ì…
        var tertiaryNames = new[]
        {
            "œÊ—…", "„ƒ „—", "Ê—‘…", "≈Ã«“…", "‰ﬁ·",
            " ‰”Ì»", " ﬁ«⁄œ", " ﬂ·Ì›", "‘Â«œ…", "Œœ„…"
        };

        var tertiaryList = new List<TertiaryCategory>();
        var targetSecs = secondaryList.Where(s =>
            s.Name == "«‰›ﬂ«ﬂ" || s.Name == "„»«‘—…" || s.Name == "«Õ ”«»" ||
            s.Name == "≈Ã«“…" || s.Name == "œÊ—…" || s.Name == " ﬂ·Ì›").ToList();

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

    // »–— Õ”«» «·„œÌ— «·«› —«÷Ì
    var adminUser = await db.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == "admin");
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

        await db.Users.AddAsync(adminUser);
        await db.SaveChangesAsync();
    }
}