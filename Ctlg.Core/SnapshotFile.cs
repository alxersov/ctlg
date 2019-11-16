using System;
namespace Ctlg.Core
{
    public sealed class SnapshotFile
    {
        public string Name { get; set; }
        public string Date { get; set; }
        public string FullPath { get; set; }

        public SnapshotFile(string name, string date, string fullPath)
        {
            Name = name;
            Date = date;
            FullPath = fullPath;
        }
    }
}
