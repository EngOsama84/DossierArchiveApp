using ClosedXML.Excel;
using DossierArchiveApp.Models;

namespace DossierArchiveApp.Services;

public class ExportService
{
    public byte[] ExportEmployeeDocumentsToExcel(Employee employee, List<Document> documents, SystemSetting? settings)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("إضبارة الموظف");

        // ضبط اتجاه الورقة من اليمين لليسار (RTL)
        worksheet.RightToLeft = true;

        // 1. ترويسة المؤسسة
        string directorate = !string.IsNullOrWhiteSpace(settings?.DirectorateName)
            ? settings.DirectorateName
            : "جمهورية العراق";
        string department = !string.IsNullOrWhiteSpace(settings?.DepartmentName)
            ? settings.DepartmentName
            : string.Empty;

        worksheet.Cell("A1").Value = directorate;
        worksheet.Cell("A1").Style.Font.Bold = true;
        worksheet.Cell("A1").Style.Font.FontSize = 13;

        if (!string.IsNullOrEmpty(department))
        {
            worksheet.Cell("A2").Value = department;
            worksheet.Cell("A2").Style.Font.Bold = true;
            worksheet.Cell("A2").Style.Font.FontSize = 11;
            worksheet.Cell("A2").Style.Font.FontColor = XLColor.DimGray;
        }

        worksheet.Cell("E1").Value = $"تاريخ الاستخراج: {DateTime.Now:yyyy/MM/dd}";
        worksheet.Cell("E1").Style.Font.FontSize = 10;
        worksheet.Cell("E1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        // 2. بطاقة بيانات الموظف
        worksheet.Range("A4:E4").Merge();
        worksheet.Cell("A4").Value = $"فهرس وثائق إضبارة الموظف: {employee.EmployeeName} (رقم الإضبارة: {employee.FileNumber})";
        worksheet.Cell("A4").Style.Font.Bold = true;
        worksheet.Cell("A4").Style.Font.FontSize = 12;
        worksheet.Cell("A4").Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F5E9");
        worksheet.Cell("A4").Style.Font.FontColor = XLColor.FromHtml("#1B4D3E");
        worksheet.Cell("A4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // 3. عناوين أعمدة الجدول (5 أعمدة أساسية)
        int headerRow = 6;
        string[] headers = { "ت", "رقم الكتاب", "تاريخ الكتاب", "الموضوع", "تاريخ الأرشفة" };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontSize = 11;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1B4D3E");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }
        worksheet.Row(headerRow).Height = 26;

        // 4. تعبئة بيانات المستندات
        int currentRow = headerRow + 1;
        int index = 1;

        foreach (var doc in documents)
        {
            worksheet.Cell(currentRow, 1).Value = index++;
            worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(currentRow, 2).Value = doc.DocumentNumber;
            worksheet.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(currentRow, 2).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 3).Value = doc.DocumentDate;
            worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(currentRow, 4).Value = doc.Subject;
            worksheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(currentRow, 5).Value = doc.CreatedAt;
            worksheet.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // تظليل خفيف للسطور الزوجية
            if (currentRow % 2 == 0)
            {
                worksheet.Range(currentRow, 1, currentRow, headers.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#F9FBF9");
            }

            currentRow++;
        }

        // 5. حدود الجدول
        var tableRange = worksheet.Range(headerRow, 1, currentRow - 1, headers.Length);
        tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
        tableRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#1B4D3E");
        tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.InsideBorderColor = XLColor.FromHtml("#D3D3D3");

        // ضبط اتساع الأعمدة تلقائياً
        worksheet.Columns().AdjustToContents();
        worksheet.Column(4).Width = Math.Max(worksheet.Column(4).Width, 40); // اتساع مريح لعمود الموضوع

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return memoryStream.ToArray();
    }
}