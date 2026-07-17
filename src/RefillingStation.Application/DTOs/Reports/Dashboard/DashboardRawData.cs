
namespace RefillingStation.Application.DTOs.Reports.Dashboard
{
    public sealed class DashboardRawData
    {
        public List<CustomerDebtReportItem> Debts { get; set; } = [];
        public List<CustomerDebtReportItem> DebtsRunning { get; set; } = [];
        public List<ExpenseReportItem> Expenses { get; set; } = [];
        public List<PayrollEntryReportItem> PayrollEntries { get; set; } = [];
        public List<PayrollEntryReportItem> PayrollEntriesRunning { get; set; } = [];
        public List<PayrollPaymentReportItem> PayrollPayments { get; set; } = [];
        public List<PayrollPaymentReportItem> PayrollPaymentsRunning { get; set; } = [];
        public List<TripReportItem> Trips { get; set; } = [];
        public List<TripReportItem> TripsBefore { get; set; } = [];

    }
}
