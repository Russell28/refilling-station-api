using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefillingStation.Application.DTOs.Reports.Breakdowns
{
    public sealed record DebtBreakdownResponse(
        decimal TotalDebt,
        IReadOnlyList<DebtBreakdownItemResponse> Items
    );
}
