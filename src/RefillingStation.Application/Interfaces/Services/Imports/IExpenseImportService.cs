using RefillingStation.Application.DTOs.Imports;

namespace RefillingStation.Application.Interfaces.Services.Imports
{
    public interface IExpenseImportService
    {
        Task<ImportResult> ImportAsync(Stream stream);

    }
}
