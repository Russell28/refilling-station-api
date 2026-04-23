namespace RefillingStation.Api.Entities
{
    public class Customer : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<CustomerDebtEntry> CustomerDebtEntries { get; set; } = new List<CustomerDebtEntry>();
    }
}
