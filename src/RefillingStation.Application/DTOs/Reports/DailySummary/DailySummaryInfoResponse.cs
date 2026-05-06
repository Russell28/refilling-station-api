using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefillingStation.Application.DTOs.Reports.DailySummary
{
    public record DailySummaryInfoResponse(
        DateOnly Date,
        int TripCount,

        decimal BacklogStartQty,
        decimal TotalCollectedQty,
        decimal TotalLoadedQty,
        decimal TotalDeliveredQty,
        decimal BacklogEndQty,

        decimal TotalFreeQty,
        decimal TotalReturnedQty,
        decimal TotalReplacementQty,

        decimal TotalCashCollected,
        decimal TotalExpenses,
        decimal TotalPayrollEarned,
        decimal TotalPayrollPaid,

        decimal TotalDebtCreatedToday,
        decimal TotalDebtPaymentsToday,
        decimal OutstandingDebt,

        decimal NetCashFlow
    );
}
