using RefillingStation.Domain.ErrorCodes;

namespace RefillingStation.Domain.Exceptions
{
    public sealed class NotFoundException : DomainException
    {
        public NotFoundException(string entityName, object key) 
            : base(DomainErrorCodes.Common.EntityNotFound, $"{entityName} with identifier '{key}' was not found.")
        {
        }
    }
}
