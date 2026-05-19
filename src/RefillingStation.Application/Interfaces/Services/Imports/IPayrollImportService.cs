using RefillingStation.Application.DTOs.Imports;

namespace RefillingStation.Application.Interfaces.Services.Imports
{
    public interface IPayrollImportService
    {
        Task<ImportResult> ImportAsync(Stream stream);
    }
}
