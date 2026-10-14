using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using DossierArchiveApp.Components;
using DossierArchiveApp.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. ≈÷«›… Œœ„«  „ﬂÊ‰«  Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 2.  ”ÃÌ· Œœ„«  „ﬂ »… MudBlazor
builder.Services.AddMudServices();

// ========================================================
// 3. „ﬂ«‰ ”ÿ— ﬁ«⁄œ… «·»Ì«‰«  »«·÷»ÿ (Â‰«):
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=dossiers.sqlite3"));
// ========================================================

var app = builder.Build();

//  ﬂÊÌ‰ „”«— «·ÿ·»«  (HTTP request pipeline)
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