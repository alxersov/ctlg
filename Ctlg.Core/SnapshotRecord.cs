using System;
namespace Ctlg.Core
{
    public class SnapshotRecord : IComparable
    {
        public SnapshotRecord()
        {
        }

        public string RelativePath { get; set; }
        public long Size { get; set; }
        public DateTime FileModifiedDateTime { get; set; }
        public byte[] Hash { get; set; }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            var otherRecord = obj as SnapshotRecord;

            return string.Compare(RelativePath, otherRecord.RelativePath, StringComparison.Ordinal);
        }
    }
}
