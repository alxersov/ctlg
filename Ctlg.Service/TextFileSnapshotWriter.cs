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

        public SnapshotRecord AddFile(File file)
        {
            var snapshotRecord = CreateSnapshotRecord(file);

            StreamWriter.WriteLine(snapshotRecord);

            return snapshotRecord;
        }

        public void Dispose()
        {
            if (StreamWriter != null)
            {
                StreamWriter.Dispose();
            }
        }

        private SnapshotRecord CreateSnapshotRecord(File file)
        {
            var hash = file.Hashes.First(h => h.HashAlgorithmId == HashAlgorithm.HashAlgorithmId);
            var date = file.FileModifiedDateTime ?? DateTime.MinValue;
            var size = file.Size ?? 0;
            var path = file.RelativePath;

            return new SnapshotRecord(hash, date, size, path);
        }
    }
}
