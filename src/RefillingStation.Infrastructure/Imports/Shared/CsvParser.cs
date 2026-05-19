using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace RefillingStation.Infrastructure.Imports.Shared
{
    public static class CsvParser
    {
        public static List<T> Parse<T>(Stream stream, ClassMap<T> map)
        {
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null,
                TrimOptions = TrimOptions.Trim
            });

            csv.Context.RegisterClassMap(map);
            return csv.GetRecords<T>().ToList();
        }
    }
}
