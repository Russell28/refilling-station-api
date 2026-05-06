using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefillingStation.Application.DTOs.Reports.MonthlySummary
{
    public sealed record MonthlySavedClosingResponse(
        decimal ManagerShare,
        decimal OwnerShare,
        string? Notes,
        DateTime CreatedAt
    );
}
