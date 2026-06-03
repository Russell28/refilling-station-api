using RefillingStation.Domain.ErrorCodes;

namespace RefillingStation.Domain.Exceptions
{
    public sealed class ConflictException : DomainException
    {
        public ConflictException(string message) 
            : base(DomainErrorCodes.CommonCode.Conflict, message)
        {
        }
    }
}
