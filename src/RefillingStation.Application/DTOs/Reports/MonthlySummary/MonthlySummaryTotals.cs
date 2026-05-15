namespace RefillingStation.Application.DTOs.Reports.MonthlySummary
{
    public sealed record MonthlySummaryTotals
    (
        decimal GrossTotal,
        decimal DebtTotal,
        decimal ExpenseTotal,
        decimal PayrollEarnedTotal,
        decimal PayrollPaidTotal,
        decimal PayrollOwedTotal,

        decimal netBeforePayroll,
        decimal netAfterPayroll,
        decimal netCashFlow
    );
}
