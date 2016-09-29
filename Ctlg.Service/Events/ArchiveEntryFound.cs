namespace Ctlg.Service.Events
{
    public class ArchiveEntryFound : IDomainEvent
    {
        public ArchiveEntryFound(string entryKey)
        {
            EntryKey = entryKey;
        }

        public string EntryKey { get; private set; }
    }
}
