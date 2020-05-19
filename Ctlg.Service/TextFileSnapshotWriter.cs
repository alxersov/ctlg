using System;
using System.IO;
using File = Ctlg.Core.File;
using Ctlg.Core.Interfaces;
using Ctlg.Core;
using System.Linq;

namespace Ctlg.Service
{
    public class TextFileSnapshotWriter: ISnapshotWriter
    {
        public TextFileSnapshotWriter(StreamWriter streamWriter, HashAlgorithm hashAlgorithm)
        {
            StreamWriter = streamWriter;
            HashAlgorithm = hashAlgorithm;
        }

        private StreamWriter StreamWriter { get; }
        public HashAlgorithm HashAlgorithm { get; }

        public void AddComment(string message)
        {
            StreamWriter.WriteLine($"# {message}");
        }

        public void AddFile(File file)
        {
            StreamWriter.WriteLine(FormatTextLine(file));
        }

        public void Dispose()
        {
            if (StreamWriter != null)
            {
                StreamWriter.Dispose();
            }
        }

        private string FormatTextLine(File file)
        {
            var hash = file.Hashes.First(h => h.HashAlgorithmId == HashAlgorithm.HashAlgorithmId);

            return $"{hash} {file.FileModifiedDateTime:o} {file.Size} {file.RelativePath}";
        }
    }
}
