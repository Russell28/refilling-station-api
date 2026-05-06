using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefillingStation.Application.DTOs.Reports.Dashboard
{
    public sealed record DashboardSummaryResponse(
        decimal TotalTrips,
        decimal BacklogStartQty,
        decimal TotalCollectedQty,
        decimal TotalLoadedQty,
        decimal TotalDeliveredQty,
        decimal BacklogEndQty,

        decimal TotalCashCollected,
        decimal TotalExpenses,
        decimal TotalPayrollPaid,
        decimal NetCashFlow,

        decimal TotalDebtCreated,
        decimal TotalDebtPayments,
        decimal OutstandingDebt,

        decimal TotalSalaryEarned,
        decimal PayrollPaid,
        decimal PayrollOwed,
        decimal OutstandingPayroll
    );
}
