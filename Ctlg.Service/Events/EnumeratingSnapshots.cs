using System;
namespace Ctlg.Service.Events
{
    public class EnumeratingSnapshots: IDomainEvent
    {
        public EnumeratingSnapshots(string name, string timestamp)
        {
            Name = name;
            Timestamp = timestamp;
        }

        public string Name { get; }
        public string Timestamp { get; }
    }
}
