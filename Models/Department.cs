namespace DossierArchiveApp.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int IsActive { get; set; } = 1;

        // علاقة 1 إلى متعدد مع الوحدات التابعة
        public ICollection<SubUnit> SubUnits { get; set; } = new List<SubUnit>();
    }
}