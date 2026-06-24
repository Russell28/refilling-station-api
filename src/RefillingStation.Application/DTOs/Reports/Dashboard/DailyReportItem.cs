namespace RefillingStation.Application.DTOs.Reports.Dashboard
{
    public sealed record DailyReportItem(
        DateOnly date,

        decimal backlogStartOfDay,
        decimal runningBacklogQty,

        int tripCountPerDay,

        decimal collectedQtyPerDay,
        decimal deliveredQtyPerDay,
        decimal freeQtyPerDay,
        decimal returnedQtyPerDay,
        decimal replacementQtyPerDay,

        decimal expensesTotalPerDay,
        decimal payrollEarnedTotalPerDay,
        decimal payrollPaidTotalPerDay,

        decimal debtCreatedPerDay,
        decimal debtPaymentsPerDay,

        decimal cashCollectedTotalPerDay,
        decimal netBeforePayrollPerDay,
        decimal netAfterPayrollPerDay
    );

}
