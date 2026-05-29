namespace RefillingStation.Domain.ErrorCodes
{
    public static class DomainErrorCodes
    {
        public static class Common
        {
            public const string EntityNotFound = "ENTITY_NOT_FOUND";
            public const string InvalidOperation = "INVALID_OPERATION";
            public const string Conflict = "CONFLICT";
            public const string RequiredField = "REQUIRED_FIELD";
        }

        public static class User
        {
            public const string AlreadyInactive = "USER_ALREADY_INACTIVE";
            public const string AlreadyActive = "USER_ALREADY_ACTIVE";
            public const string UsernameExists = "USERNAME_ALREADY_EXISTS";
            public const string InvalidPassword = "USER_INVALID_PASSWORD";
        }
    }
}
