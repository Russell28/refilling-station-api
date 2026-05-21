using RefillingStation.Application.DTOs.Imports;

namespace RefillingStation.Application.Interfaces.Services.Imports
{
    public interface ICustomerDebtImportService
    {
        Task<ImportResult> ImportAsync(Stream stream);
    }
}
