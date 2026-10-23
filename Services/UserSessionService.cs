using DossierArchiveApp.Data;
using DossierArchiveApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace DossierArchiveApp.Services;

public class UserSessionService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private static readonly PasswordHasher<User> _passwordHasher = new();

    public int? LastSelectedEmployeeId { get; set; }
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
            if (IsSuperAdmin) return "مدير عام النظام (وصول شامل لكافة الشُعب)";
            if (CurrentUser.IsAdmin == 1) return $"مدير شعبة ({CurrentDepartmentName})";
            return $"مستخدم شعبة ({CurrentDepartmentName})";
        }
    }

    // دوال فحص الصلاحيات المباشرة
    public bool IsAdmin => CurrentUser != null && CurrentUser.IsAdmin == 1;
    public bool CanAccessSettingsOrUsers => IsAdmin;

    // خصائص عزل البيانات والشُعب
    public int? CurrentDepartmentId => CurrentUser?.DepartmentId;
    public string CurrentDepartmentName => CurrentUser?.Department?.Name ?? "المديرية العامة (مستقل)";
    public string CurrentUserDisplayName =>
    !string.IsNullOrWhiteSpace(CurrentUser?.FullName)
        ? CurrentUser.FullName
        : (CurrentUser?.UserName ?? "مستخدم");
    /// <summary>
    /// تحديد ما إذا كان المستخدم يمتلك صلاحية الرؤية العامة لكافة الشُعب والأضابير
    /// </summary>
    public bool IsSuperAdmin => CurrentUser != null &&
                               ((CurrentUser.IsAdmin == 1 && CurrentUser.IsSuperAdmin == 1) ||
                                CurrentUser.UserName.ToLower() == "admin");

    /// <summary>
    /// التحقق مما إذا كان للمستخدم صلاحية عرض أو تعديل إضبارة موظف محدد
    /// </summary>
    public bool CanAccessEmployee(Employee? employee)
    {
        if (employee == null) return false;
        if (IsSuperAdmin) return true;
        return employee.DepartmentId == CurrentDepartmentId;
    }

    public bool CanAdd => CurrentUser != null && (CurrentUser.IsAdmin == 1 || CurrentUser.CanAdd == 1);
    public bool CanEdit => CurrentUser != null && (CurrentUser.IsAdmin == 1 || CurrentUser.CanEdit == 1);
    public bool CanDelete => CurrentUser != null && (CurrentUser.IsAdmin == 1 || CurrentUser.CanDelete == 1);
    public bool CanExport => CurrentUser != null && (CurrentUser.IsAdmin == 1 || CurrentUser.CanExport == 1);

    // صلاحيات إدارة الموظفين
    public bool CanAddEmployee => CanAdd;
    public bool CanEditEmployee => CanEdit;
    public bool CanDeleteEmployee => CanDelete;

    /// <summary>
    /// تجزئة كلمة المرور باستخدام معيار PBKDF2 المدمج في ASP.NET Core مع Salt تلقائي
    /// </summary>
    public static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) return string.Empty;
        return _passwordHasher.HashPassword(new User(), password);
    }

    /// <summary>
    /// التحقق الآمن من كلمة المرور عبر PBKDF2 وتحميل الشُعبة التابع لها المستخدم
    /// </summary>
    public async Task<User?> ValidateUserCredentialsAsync(string username, string password)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await db.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.UserName.ToLower() == username.Trim().ToLower() && u.IsActive == 1);

        if (user == null || string.IsNullOrEmpty(user.PasswordHash)) return null;

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (verificationResult == PasswordVerificationResult.Success)
        {
            return user;
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            user.UpdatedAt = DateTime.Now;
            await db.SaveChangesAsync();
            return user;
        }

        return null;
    }

    // تسجيل دخول المستخدم وتفعيل الجلسة
    public async Task SetUserAsync(User user)
    {
        CurrentUser = user;
        NotifySessionChanged();
        await LogActivityAsync("تسجيل دخول", "النظام", user.UserId, $"قام المستخدم ({user.UserName}) التابع لـ ({CurrentDepartmentName}) بتسجيل الدخول.");
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

    // دالة توثيق الحركات
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
                CreatedAt = DateTime.Now
            };

            db.AuditLogs.Add(log);
            await db.SaveChangesAsync();
        }
        catch { }
    }
}