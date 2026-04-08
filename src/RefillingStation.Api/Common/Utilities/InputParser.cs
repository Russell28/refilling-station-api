namespace RefillingStation.Api.Common.Utilities
{
    public static class InputParser
    {
        public static string ParseRequiredString(string? value, string fieldName)
        {
            if (string.IsNullOrEmpty(value))
                throw new Exception($"{fieldName} is required.");

            return value.Trim();
        }

        public static string? ParseOptionalString(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        public static DateOnly ParseRequiredDate(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new Exception($"{fieldName} is required.");

            if (!DateOnly.TryParse(value, out var parsed))
                throw new Exception($"{fieldName} must be a valid date.");

            return parsed;
        }

        public static int ParseRequiredInt(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception($"{fieldName} is required.");
            }

            if (!int.TryParse(value, out var parsedValue))
            {
                throw new Exception($"{fieldName} must be a valid whole number.");
            }

            if (parsedValue <= 0)
            {
                throw new Exception($"{fieldName} must be greater than 0.");
            }

            return parsedValue;
        }

        public static decimal ParseNonNegativeDecimal(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            if (!decimal.TryParse(value, out var parsedValue))
            {
                throw new Exception($"{fieldName} must be a valid number.");
            }

            if (parsedValue < 0)
            {
                throw new Exception($"{fieldName} cannot be negative.");
            }

            return parsedValue;
        }

        public static decimal ParseRequiredDecimal(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            if (!decimal.TryParse(value, out var parsedValue))
            {
                throw new Exception($"{fieldName} must be a valid number.");
            }

            return parsedValue;
        }
    }
}
