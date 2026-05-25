namespace RefillingStation.Domain.Exceptions
{
    public sealed class NotFoundException : DomainException
    {
        public NotFoundException(string entityName, object key) 
            : base($"{entityName} with identifier '{key}' was not found.")
        {
        }
    }
}
