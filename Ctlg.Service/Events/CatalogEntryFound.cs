using Ctlg.Core;

namespace Ctlg.Service.Events
{
    public class CatalogEntryFound : IDomainEvent
    {
        public CatalogEntryFound(File entry)
        {
            Entry = entry;
        }

        public File Entry { get; }
    }
}
