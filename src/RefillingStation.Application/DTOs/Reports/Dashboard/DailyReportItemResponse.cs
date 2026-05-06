using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefillingStation.Application.DTOs.Reports.Dashboard
{
    public sealed record DailyReportItemResponse(
        DateOnly Date,
        decimal TripCount,
        decimal CollectedQty,
        decimal DeliveredQty,
        decimal CashCollected,
        decimal Expenses,
        decimal PayrollPaid,
        decimal DebtCreated,
        decimal DebtPayments,
        decimal NetCashFlow,
        decimal BacklogEndQty
    );
}
