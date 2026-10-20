using DossierArchiveApp.Data;
using DossierArchiveApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Text;

namespace DossierArchiveApp.Services;

public class UserSessionService
{
    private readonly IServiceScopeFactory _scopeFactory;

    // المستخدم النشط حالياً (null يعني لم يتم تسجيل الدخول بعد)
    public User? CurrentUser { get; private set; }

    // التحقق هل هناك مستخدم مسجل الدخول
    public bool IsLoggedIn => CurrentUser != null;

    // أحداث إشعار الواجهات بالتحديث عند الدخول أو الخروج
    public event Action? OnSessionChanged;
    public event Action? OnChange;

    public UserSessionService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        CurrentUser = null;
    }

    // المسمى الوصفي للصلاحية
    public string RoleName
    {
        get
        {
            if (CurrentUser == null) return string.Empty;
            if (CurrentUser.IsAdmin == 1) return "مدير نظام (تحكم كامل)";
            return "مستخدم مخصص الصلاحيات";
        }
    }

    // دوال فحص الصلاحيات المباشرة
    public bool IsAdmin => CurrentUser != null && CurrentUser.IsAdmin == 1;
    public bool CanAccessSettingsOrUsers => IsAdmin;

    public bool CanAdd => CurrentUser != null && (CurrentUser.IsAdmin == 1 || CurrentUser.CanAdd == 1);
    public bool CanEdit => CurrentUser != null && (CurrentUser.IsAdmin == 1 || CurrentUser.CanEdit == 1);
    public bool CanDelete => CurrentUser != null && (CurrentUser.IsAdmin == 1 || CurrentUser.CanDelete == 1);
    public bool CanExport => CurrentUser != null && (CurrentUser.IsAdmin == 1 || CurrentUser.CanExport == 1);

    // صلاحيات إدارة الموظفين
    public bool CanAddEmployee => CanAdd;
    public bool CanEditEmployee => CanEdit;
    public bool CanDeleteEmployee => CanDelete;

    /// <summary>
    /// دالة تشفير النصوص الثابتة باستخدام SHA-256
    /// </summary>
    public static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) return string.Empty;

        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var builder = new StringBuilder();
        foreach (var b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }

    /// <summary>
    /// التحقق الذكي من كلمة المرور مع ترقية الحسابات القديمة تلقائياً إلى مشفرة
    /// </summary>
    public async Task<User?> ValidateUserCredentialsAsync(string username, string password)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == username && u.IsActive == 1);
        if (user == null) return null;

        string hashedInput = HashPassword(password);
        bool isValid = false;

        // 1. فحص هل كلمة المرور في القاعدة مشفرة وتطابق المدخل
        if (user.PasswordHash == hashedInput)
        {
            isValid = true;
        }
        // 2. التوافق العكسي: إذا كانت الكلمة القديمة غير مشفرة وطابقت النص الصريح
        else if (user.PasswordHash == password)
        {
            isValid = true;
            // ترقية كلمة المرور وحفظها مشفرة في القاعدة فوراً
            user.PasswordHash = hashedInput;
            user.UpdatedAt = DateTime.Now;
            await db.SaveChangesAsync();
        }

        return isValid ? user : null;
    }

    // تسجيل دخول المستخدم وتفعيل الجلسة
    public async Task SetUserAsync(User user)
    {
        CurrentUser = user;
        NotifySessionChanged();
        await LogActivityAsync("تسجيل دخول", "النظام", user.UserId, $"قام المستخدم ({user.UserName}) بتسجيل الدخول للنظام.");
    }

    // تسجيل خروج المستخدم وإنهاء الجلسة
    public async Task LogLogoutAsync()
    {
        if (CurrentUser != null)
        {
            await LogActivityAsync("تسجيل خروج", "النظام", CurrentUser.UserId, $"قام المستخدم ({CurrentUser.UserName}) بتسجيل الخروج.");
            CurrentUser = null;
            NotifySessionChanged();
        }
    }

    private void NotifySessionChanged()
    {
        OnSessionChanged?.Invoke();
        OnChange?.Invoke();
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
            // تجاوز الخطأ لتفادي تعطيل تنفيذ العمليات الرئيسية
        }
    }
}