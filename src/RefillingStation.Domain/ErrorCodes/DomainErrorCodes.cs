namespace RefillingStation.Domain.ErrorCodes
{
    public static class DomainErrorCodes
    {
        public static class CommonCode
        {
            public const string EntityNotFound = "ENTITY_NOT_FOUND";
            public const string InvalidOperation = "INVALID_OPERATION";
            public const string Conflict = "CONFLICT";
            public const string RequiredField = "REQUIRED_FIELD";
        }

        public static class UserCode
        {
            public const string AlreadyInactive = "USER_ALREADY_INACTIVE";
            public const string AlreadyActive = "USER_ALREADY_ACTIVE";
            public const string UsernameExists = "USERNAME_ALREADY_EXISTS";
            public const string InvalidPassword = "USER_INVALID_PASSWORD";
            public const string InvalidUsernameLength = "USERNAME_LENGTH_INVALID";
            public const string InvalidUserRole = "USER_INVALID_ROLE";
            public const string CannotDeleteSelf = "USER_INVALID_SELF_DELETE";
        }

        public static class RefreshTokenCode
        {
            public const string InvalidToken = "REFRESHTOKEN_INVALID_TOKEN";
            public const string InvalidTimestamp = "REFRESHTOKEN_INVALID_TIMESTAMP";
            public const string AlreadyRevoked = "REFRESHTOKEN_ALREADY_REVOKED";
        }
    }
}
