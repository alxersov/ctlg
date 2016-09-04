using System;
using System.Collections.Generic;

namespace Ctlg.Data.Model
{
    public class File
    {
        public File()
        {
            Contents = new List<File>();
            Hashes = new List<Hash>();
        }

        public File(string name, bool isDirectory = false)
        {
            Contents = new List<File>();
            Hashes = new List<Hash>();

            Name = name;
            IsDirectory = isDirectory;
            RecordUpdatedDateTime = DateTime.UtcNow;
        }

        public int FileId { get; set; }
        public int? ParentFileId { get; set; }
        public File ParentFile { get; set; }
        public bool IsDirectory { get; set; }
        public string Name { get; set; }
        public long? Size { get; set; }
        public DateTime? FileCreatedDateTime { get; set; }
        public DateTime? FileModifiedDateTime { get; set; }
        public DateTime RecordUpdatedDateTime { get; set; }

        public IList<File> Contents { get; set; }
        public IList<Hash> Hashes { get; set; }

        public string FullPath { get; set; }

        public string BuildFullPath()
        {
            if (FullPath == null)
            {
                if (ParentFile == null && ParentFileId != null)
                {
                    throw new InvalidOperationException("BuildFullPath failed because ParentFile is not loaded.");
                }

                if (ParentFile == null)
                {
                    FullPath = Name;
                }
                else
                {
                    FullPath = string.Format("{0}\\{1}", ParentFile.BuildFullPath(), Name);
                }
            }

            return FullPath;
        }
    }
}
