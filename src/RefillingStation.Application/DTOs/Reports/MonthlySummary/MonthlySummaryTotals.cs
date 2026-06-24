namespace RefillingStation.Application.DTOs.Reports.MonthlySummary
{
    public sealed record MonthlySummaryTotals
    (
        decimal cashCollected,
        decimal debtTotal,
        decimal expenseTotal,
        decimal payrollEarnedTotal,
        decimal payrollPaidTotal,
        decimal payrollOwedTotal,

        decimal netBeforePayroll,
        decimal netAfterPayroll,
        decimal netCashFlow
    );
}
