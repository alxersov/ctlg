using Ctlg.Core;

namespace Ctlg.Service.Events
{
    public class TreeItemEnumerated : IDomainEvent
    {
        public TreeItemEnumerated(File file, int nestingLevel)
        {
            File = file;
            NestingLevel = nestingLevel;
        }

        public int NestingLevel { get; }
        public File File { get; }
    }
}
