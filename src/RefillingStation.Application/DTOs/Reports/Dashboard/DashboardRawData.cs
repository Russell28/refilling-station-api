using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.DTOs.Reports.Dashboard
{
    public sealed class DashboardRawData
    {
        public List<CustomerDebtReportItem> Debts { get; set; } = [];
        public List<CustomerDebtReportItem> DebtsRunning { get; set; } = [];
        public List<ExpenseReportItem> Expenses { get; set; } = [];
        public List<PayrollReportItem> Payrolls { get; set; } = [];
        public List<PayrollReportItem> PayrollsRunning { get; set; } = [];
        public List<TripReportItem> Trips { get; set; } = [];
        public List<TripReportItem> TripsBefore { get; set; } = [];

    }
}
