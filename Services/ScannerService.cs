using System.Runtime.InteropServices;

namespace DossierArchiveApp.Services;

public class ScannerInfo
{
    public string DeviceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class ScannerService
{
    /// <summary>
    /// قراءة أجهزة الماسح الضوئي المعرفة على نظام Windows عبر مكتبة WIA
    /// </summary>
    public List<ScannerInfo> GetConnectedScanners()
    {
        var scanners = new List<ScannerInfo>();

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return scanners;

        try
        {
            dynamic deviceManager = Activator.CreateInstance(Type.GetTypeFromProgID("WIA.DeviceManager")!);
            if (deviceManager == null) return scanners;

            dynamic deviceInfos = deviceManager.DeviceInfos;
            int count = deviceInfos.Count;

            for (int i = 1; i <= count; i++)
            {
                dynamic info = deviceInfos[i];
                // النوع 1 في نظام WIA هو Scanner Device
                if ((int)info.Type == 1)
                {
                    scanners.Add(new ScannerInfo
                    {
                        DeviceId = (string)info.DeviceID,
                        Name = (string)info.Properties["Name"].Value
                    });
                }
            }
        }
        catch
        {
            // في حال عدم توفر خدمة WIA أو عدم ربط أي جهاز
        }

        return scanners;
    }
}