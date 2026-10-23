namespace DossierArchiveApp.Models
{
    public class SubUnit
    {
        public int SubUnitId { get; set; }
        public string Name { get; set; } = string.Empty;

        // يمكن أن تتبع شعبة، أو تكون مستقلة (null)
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public int IsActive { get; set; } = 1;
    }
}