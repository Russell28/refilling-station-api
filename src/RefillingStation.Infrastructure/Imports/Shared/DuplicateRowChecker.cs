using RefillingStation.Application.DTOs.Imports;

namespace RefillingStation.Infrastructure.Imports.Shared
{
    public static class DuplicateRowChecker
    {
        public static void AddDuplicateErrors<T>(
        IEnumerable<T> rows,
        Func<T, string> keySelector,
        Func<T, int> rowNumberSelector,
        List<ImportError> errors)
        {
            var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                var key = keySelector(row).Trim();
                var rowNum = rowNumberSelector(row);

                if (seen.TryGetValue(key, out var firstRow))
                {
                    errors.Add(new ImportError
                    {
                        RowNumber = rowNum,
                        Message = $"Duplicate entry '{key}'. First seen at row {firstRow}."
                    });
                }
                else
                {
                    seen[key] = rowNum;
                }
            }
        }
    }
}
