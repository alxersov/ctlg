namespace Ctlg.Service.Events
{
    public class CatalogEntryNotFound : IDomainEvent
    {
        public CatalogEntryNotFound(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }
}
