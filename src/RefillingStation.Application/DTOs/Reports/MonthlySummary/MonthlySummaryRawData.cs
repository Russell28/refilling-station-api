namespace RefillingStation.Application.DTOs.Reports.MonthlySummary
{
    public sealed class MonthlySummaryRawData
    {
        public decimal DebtTotal { get; set; }
        public decimal ExpenseTotal { get; set; }
        public decimal PayrollEarnedTotal { get; set; }
        public decimal PayrollPaidTotal { get; set; }
        public decimal GrossTotal { get; set; }
        public MonthlyClosingReportItem? SavedClosing { get; set; } 
    }
}
