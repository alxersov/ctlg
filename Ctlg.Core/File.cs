using System;
using System.Collections.Generic;

namespace Ctlg.Core
{
    public class File
    {
        public File()
        {
            Contents = new List<File>();
            Hashes = new List<Hash>();
            RelativePath = string.Empty;
        }

        public File(string name, bool isDirectory = false)
        {
            Contents = new List<File>();
            Hashes = new List<Hash>();
            RelativePath = string.Empty;
            Name = name;

            IsDirectory = isDirectory;
            RecordUpdatedDateTime = DateTime.UtcNow;
        }

        public int FileId { get; set; }
        public int? ParentFileId { get; set; }
        public File ParentFile { get; set; }
        public bool IsDirectory { get; set; }
        public bool IsRoot { get { return string.IsNullOrEmpty(RelativePath); }}
        public string Name { get; set; }
        public long? Size { get; set; }
        public DateTime? FileCreatedDateTime { get; set; }
        public DateTime? FileModifiedDateTime { get; set; }
        public DateTime RecordUpdatedDateTime { get; set; }

        public List<File> Contents { get; set; }
        public IList<Hash> Hashes { get; set; }

        public string RelativePath { get; set; }
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
                    // TODO: use CombinePath method
                    FullPath = string.Format("{0}\\{1}", ParentFile.BuildFullPath(), Name);
                }
            }

            return FullPath;
        }

        public void SortTree()
        {
            Contents.Sort(FileNameComparer);

            foreach (var file in Contents)
            {
                file.SortTree();
            }
        }

        public File GetInnerFile(string name)
        {
            var index = Contents.BinarySearch(new File(name), FileNameComparer);
            if (index < 0)
            {
                return null;
            }

            return Contents[index];
        }

        private IComparer<File> FileNameComparer { get; } = new FileNameComparer();
    }
}
