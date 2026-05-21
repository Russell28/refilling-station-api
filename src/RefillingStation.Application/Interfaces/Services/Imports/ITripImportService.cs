using RefillingStation.Application.DTOs.Imports;

namespace RefillingStation.Application.Interfaces.Services.Imports
{
    public interface ITripImportService
    {
        Task<ImportResult> ImportAsync(Stream stream);
    }
}
