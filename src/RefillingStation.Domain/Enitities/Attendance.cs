using RefillingStation.Domain.Entities;

namespace RefillingStation.Domain.Enitities
{
    public class Attendance : BaseEntity
    {
        public DateOnly Date { get; set; }
        public int EmployeeId { get; set; }
        public bool IsPresent { get; set; }
        public DateTime? TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }

        public Employee Employee { get; set; } = null!;
    }
}
