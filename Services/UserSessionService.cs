using DossierArchiveApp.Data;
using DossierArchiveApp.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DossierArchiveApp.Services;

public class UserSessionService
{
    private readonly IServiceScopeFactory _scopeFactory;

    // المستخدم النشط حالياً (null يعني لم يتم تسجيل الدخول بعد)
    public User? CurrentUser { get; private set; }

    // التحقق هل هناك مستخدم مسجل الدخول
    public bool IsLoggedIn => CurrentUser != null;

    // حدث لإشعار الواجهات (مثل MainLayout و NavMenu) بالتحديث عند الدخول أو الخروج
    public event Action? OnSessionChanged;

    public UserSessionService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        CurrentUser = null; // يبدأ فارغاً لإجبار المستخدم على تسجيل الدخول أولاً
    }

    // المسمى الوصفي للصلاحية
    public string RoleName => CurrentUser?.UserTypeId switch
    {
        1 => "مدير نظام (تحكم كامل)",
        2 => "محرر بيانات (إضافة، تعديل، حذف)",
        3 => "مدخل بيانات (إضافة وتعديل)",
        4 => "قارئ بيانات (استعراض فقط)",
        _ => string.Empty
    };

    // دوال فحص الصلاحيات المباشرة
    public bool IsAdmin => CurrentUser?.UserTypeId == 1;
    public bool CanAdd => CurrentUser?.UserTypeId is 1 or 2 or 3;
    public bool CanEdit => CurrentUser?.UserTypeId is 1 or 2 or 3;
    public bool CanDelete => CurrentUser?.UserTypeId is 1 or 2;
    public bool CanAccessSettingsOrUsers => CurrentUser?.UserTypeId == 1;
    // صلاحيات إدارة الموظفين
    public bool CanAddEmployee => CurrentUser?.UserTypeId is 1 or 2 or 3;
    public bool CanEditEmployee => CurrentUser?.UserTypeId is 1 or 2 or 3;
    public bool CanDeleteEmployee => CurrentUser?.UserTypeId is 1 or 2;
    // تسجيل دخول المستخدم وتفعيل الجلسة
    public async Task SetUserAsync(User user)
    {
        CurrentUser = user;
        OnSessionChanged?.Invoke();
        await LogActivityAsync("تسجيل دخول", "النظام", user.UserId, $"قام المستخدم ({user.UserName}) بتسجيل الدخول للنظام.");
    }

    // تسجيل خروج المستخدم وإنهاء الجلسة
    public async Task LogLogoutAsync()
    {
        if (CurrentUser != null)
        {
            await LogActivityAsync("تسجيل خروج", "النظام", CurrentUser.UserId, $"قام المستخدم ({CurrentUser.UserName}) بتسجيل الخروج.");
            CurrentUser = null;
            OnSessionChanged?.Invoke();
        }
    }

    // دالة مركزية لتوثيق أي حركة في جدول TB_AuditLog فوراً
    public async Task LogActivityAsync(string actionType, string entityType, int? entityId, string description)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var log = new AuditLog
            {
                UserId = CurrentUser?.UserId,
                ActionType = actionType,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            db.AuditLogs.Add(log);
            await db.SaveChangesAsync();
        }
        catch
        {
            // تجاوز الخطأ لتفادي تعطيل تنفيذ العمليات الرئيسية في حال تعثر كتابة السجل
        }
    }
}