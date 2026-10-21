using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using MudBlazor;

namespace DossierArchiveApp.Services;

/// <summary>
/// نموذج بيانات جهاز الماسح الضوئي المستخدم في شاشة الإعدادات
/// </summary>
public class ScannerInfo
{
    public string DeviceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class ScannerService
{
    private readonly ISnackbar _snackbar;
    private readonly string? _naps2Path;

    public ScannerService(ISnackbar snackbar, IConfiguration configuration)
    {
        _snackbar = snackbar;
        _naps2Path = ResolveNaps2Path(configuration);
    }

    /// <summary>
    /// حل مسار NAPS2 بدقة عبر عدة مسارات محتملة وتجنب مشاكل المسار النسبي وحساسية الأحرف
    /// </summary>
    private static string? ResolveNaps2Path(IConfiguration configuration)
    {
        // 1. المسار المعرف في appsettings.json إن وجد
        var configuredPath = configuration["ScannerSettings:Naps2ExecutablePath"];
        if (!string.IsNullOrWhiteSpace(configuredPath) && File.Exists(configuredPath))
        {
            return configuredPath;
        }

        string baseDir = AppContext.BaseDirectory;

        // 2. الفحص في مسارات التطبيق الرسمية
        string[] candidatePaths =
        {
            Path.Combine(baseDir, "NAPS2.Console.exe"),
            Path.Combine(baseDir, "naps2.console.exe"),
            Path.Combine(baseDir, "Tools", "NAPS2", "NAPS2.Console.exe"),
            Path.Combine(Directory.GetCurrentDirectory(), "NAPS2.Console.exe"),
            Path.Combine(Directory.GetCurrentDirectory(), "naps2.console.exe"),
            // مسارات التثبيت الافتراضية على الويندوز في حال تثبيته في النظام
            @"C:\Program Files\NAPS2\NAPS2.Console.exe",
            @"C:\Program Files (x86)\NAPS2\NAPS2.Console.exe"
        };

        foreach (var path in candidatePaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }

    /// <summary>
    /// جلب قائمة أجهزة السكنر المعرفة باستخدام NAPS2 عبر فحص بروتوكول WIA و TWAIN
    /// </summary>
    public List<ScannerInfo> GetConnectedScanners()
    {
        var scanners = new List<ScannerInfo>();

        if (string.IsNullOrEmpty(_naps2Path) || !File.Exists(_naps2Path))
        {
            _snackbar.Add("لم يتم العثور على أداة NAPS2.Console.exe في مسارات النظام أو مجلد التطبيق!", Severity.Error);
            return scanners;
        }

        string[] drivers = { "wia", "twain" };

        foreach (var driver in drivers)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = _naps2Path,
                    Arguments = $"--driver {driver} --listdevices",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process != null)
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(6000);

                    var lines = output.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        string trimmed = line.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmed)
                            && !trimmed.StartsWith("NAPS2", StringComparison.OrdinalIgnoreCase)
                            && !scanners.Any(s => s.Name.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
                        {
                            scanners.Add(new ScannerInfo
                            {
                                DeviceId = trimmed,
                                Name = trimmed
                            });
                        }
                    }
                }
            }
            catch
            {
                // الاستمرار لتجربة المشغل الآخر
            }
        }

        return scanners;
    }

    /// <summary>
    /// تنفيذ المسح الضوئي وحفظ النتيجة كـ PDF
    /// </summary>
    public async Task<byte[]?> ScanDocumentAsync(string? scannerName, string scanSource = "Feeder", string colorMode = "Color", int dpi = 200, bool isDuplex = false)
    {
        if (string.IsNullOrEmpty(_naps2Path) || !File.Exists(_naps2Path))
        {
            _snackbar.Add("أداة المسح NAPS2.Console.exe غير متوفرة!", Severity.Error);
            return null;
        }

        if (string.IsNullOrWhiteSpace(scannerName))
        {
            _snackbar.Add("يرجى اختيار جهاز الماسح الضوئي أولاً من الإعدادات!", Severity.Warning);
            return null;
        }

        return await Task.Run(() =>
        {
            string tempPdfPath = Path.Combine(Path.GetTempPath(), $"scan_{Guid.NewGuid():N}.pdf");

            try
            {
                string sourceArg = scanSource.Equals("Feeder", StringComparison.OrdinalIgnoreCase) ? "feeder" : "glass";

                if (sourceArg == "feeder" && isDuplex)
                {
                    sourceArg = "duplex";
                }

                // 1. المحاولة عبر WIA
                string arguments = $"-o \"{tempPdfPath}\" --driver wia --device \"{scannerName}\" --source {sourceArg} --pagesize a4 --dpi {dpi} -f";

                var psi = new ProcessStartInfo
                {
                    FileName = _naps2Path,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    if (process != null)
                    {
                        process.WaitForExit(90000);

                        if (File.Exists(tempPdfPath) && new FileInfo(tempPdfPath).Length > 0)
                        {
                            return File.ReadAllBytes(tempPdfPath);
                        }
                    }
                }

                // 2. المحاولة عبر TWAIN في حال فشل WIA
                arguments = $"-o \"{tempPdfPath}\" --driver twain --device \"{scannerName}\" --source {sourceArg} --pagesize a4 --dpi {dpi} -f";
                psi.Arguments = arguments;

                using (var processTwain = Process.Start(psi))
                {
                    if (processTwain != null)
                    {
                        string twainError = processTwain.StandardError.ReadToEnd();
                        processTwain.WaitForExit(90000);

                        if (File.Exists(tempPdfPath) && new FileInfo(tempPdfPath).Length > 0)
                        {
                            return File.ReadAllBytes(tempPdfPath);
                        }

                        if (!string.IsNullOrWhiteSpace(twainError))
                        {
                            _snackbar.Add($"خطأ السكنر: {twainError}", Severity.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _snackbar.Add($"تعذر إتمام المسح: {ex.Message}", Severity.Error);
            }
            finally
            {
                if (File.Exists(tempPdfPath))
                {
                    try { File.Delete(tempPdfPath); } catch { }
                }
            }

            return null;
        });
    }
}