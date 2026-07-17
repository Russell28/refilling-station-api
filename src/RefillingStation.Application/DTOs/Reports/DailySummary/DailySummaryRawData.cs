
namespace RefillingStation.Application.DTOs.Reports.DailySummary
{
    public sealed class DailySummaryRawData
    {
        public List<CustomerDebtReportItem> Debts { get; set; } = [];
        public List<CustomerDebtReportItem> DebtsRunning { get; set; } = [];
        public List<ExpenseReportItem> Expenses { get; set; } = [];
        public List<PayrollEntryReportItem> Payrolls { get; set; } = [];
        public List<PayrollEntryReportItem> PayrollsRunning { get; set; } = [];
        public List<TripReportItem> Trips { get; set; } = [];
        public List<TripReportItem> TripsBefore { get; set; } = [];

    }
}
