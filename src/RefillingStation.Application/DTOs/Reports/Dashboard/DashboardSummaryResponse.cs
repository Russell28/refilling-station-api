namespace RefillingStation.Application.DTOs.Reports.Dashboard
{
    public sealed record DashboardSummaryResponse(
        decimal backlogStartQty,
        decimal backlogEndQty,

        decimal tripCount,
        decimal collectedQtyTotal,
        decimal deliveredQtyTotal,

        decimal expensesTotal,

        decimal payrollEarnedTotal,
        decimal payrollPaidTotal,
        decimal outstandingPayroll,

        decimal debtCreatedTotal,
        decimal debtPaymentsTotal,
        decimal outstandingDebt,

        decimal cashCollectedTotal,
        decimal netBeforePayroll,
        decimal netAfterPayroll,
        decimal netCashFlow,

        decimal costPerGal,
        decimal retailPerGal,
        decimal profitPerGal,
        decimal salaryPaidPerGal
    );
}
