using System;
namespace Ctlg.Service.Events
{
    public class EnumeratingHashes : IDomainEvent
    {
        public EnumeratingHashes(string prefix)
        {
            Prefix = prefix;
        }

        public string Prefix { get; }
    }
}
