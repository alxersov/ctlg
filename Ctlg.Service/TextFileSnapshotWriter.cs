using System;
using System.IO;
using File = Ctlg.Core.File;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Utils;

namespace Ctlg.Service
{
    public class TextFileSnapshotWriter: ISnapshotWriter
    {
        public TextFileSnapshotWriter(StreamWriter streamWriter)
        {
            StreamWriter = streamWriter;
        }

        private StreamWriter StreamWriter { get; }

        public void AddComment(string message)
        {
            StreamWriter.WriteLine($"# {message}");
        }

        public void AddFile(File file, byte[] hash)
        {
            StreamWriter.WriteLine(FormatTextLine(file, hash));
        }

        public void Dispose()
        {
            if (StreamWriter != null)
            {
                StreamWriter.Dispose();
            }
        }

        private string FormatTextLine(File file, byte[] hash)
        {
            return $"{FormatBytes.ToHexString(hash)} {file.FileModifiedDateTime:o} {file.Size} {file.RelativePath}";
        }
    }
}
