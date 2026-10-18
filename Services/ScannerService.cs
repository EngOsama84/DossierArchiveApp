using System.Diagnostics;
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
    private readonly string _naps2Path;

    public ScannerService(ISnackbar snackbar)
    {
        _snackbar = snackbar;
        // ملف الأداة التنفيذي يقع بجانب ملف قاعدة البيانات ومجلد المشروع
        _naps2Path = Path.Combine(Directory.GetCurrentDirectory(), "naps2.console.exe");
    }

    /// <summary>
    /// جلب قائمة أجهزة السكنر المعرفة باستخدام NAPS2 عبر فحص بروتوكول WIA و TWAIN
    /// </summary>
    public List<ScannerInfo> GetConnectedScanners()
    {
        var scanners = new List<ScannerInfo>();

        if (!File.Exists(_naps2Path))
        {
            _snackbar.Add("لم يتم العثور على ملف naps2.console.exe بجانب قاعدة البيانات!", Severity.Error);
            return scanners;
        }

        // فحص أجهزة WIA أولاً ثم أجهزة TWAIN
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
                // الاستمرار لتجربة الدرايفر الآخر في حال فشل أحدهما
            }
        }

        return scanners;
    }

    /// <summary>
    /// تنفيذ المسح الضوئي الفعلي وسحب الأوراق المباشر من الفيدر أو الزجاج وحفظها كـ PDF
    /// </summary>
    public async Task<byte[]?> ScanDocumentAsync(string? scannerName, string scanSource = "Feeder", string colorMode = "Color", int dpi = 200, bool isDuplex = false)
    {
        if (!File.Exists(_naps2Path))
        {
            _snackbar.Add("ملف naps2.console.exe غير موجود في مجلد النظام!", Severity.Error);
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
                // تحديد مصدر التغذية: feeder = سحب آلي من الفيدر، glass = السطح الزجاجي
                string sourceArg = scanSource.Equals("Feeder", StringComparison.OrdinalIgnoreCase) ? "feeder" : "glass";

                if (sourceArg == "feeder" && isDuplex)
                {
                    sourceArg = "duplex";
                }

                // محاولة المسح أولاً عبر WIA مع استخدام --device و --pagesize a4
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

                using var process = Process.Start(psi);
                if (process != null)
                {
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit(90000);

                    if (File.Exists(tempPdfPath) && new FileInfo(tempPdfPath).Length > 0)
                    {
                        return File.ReadAllBytes(tempPdfPath);
                    }

                    // إذا لم ينجح عبر WIA، المحاولة تلقائياً عبر TWAIN
                    arguments = $"-o \"{tempPdfPath}\" --driver twain --device \"{scannerName}\" --source {sourceArg} --pagesize a4 --dpi {dpi} -f";
                    psi.Arguments = arguments;

                    using var processTwain = Process.Start(psi);
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