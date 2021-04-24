using System;

namespace Ctlg.Core
{
    public class File
    {
        public File(string name = null)
        {
            RelativePath = string.Empty;
            Name = name;
        }

        public bool IsDirectory { get; set; }
        public string Name { get; set; }
        public long? Size { get; set; }
        public DateTime? FileModifiedDateTime { get; set; }


        public string RelativePath { get; set; }
        public string FullPath { get; set; }
    }
}
