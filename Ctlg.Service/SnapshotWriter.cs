﻿using System;
using System.IO;
using File = Ctlg.Core.File;
using Ctlg.Core.Interfaces;
using Ctlg.Core;
using System.Linq;

namespace Ctlg.Service
{
    public class SnapshotWriter: ISnapshotWriter
    {
        public SnapshotWriter(StreamWriter streamWriter)
        {
            StreamWriter = streamWriter;
        }

        private StreamWriter StreamWriter { get; }

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
            var hash = file.Hashes.First(h => h.HashAlgorithmId == (int)HashAlgorithmId.SHA256);
            var date = file.FileModifiedDateTime ?? DateTime.MinValue;
            var size = file.Size ?? 0;
            var path = file.RelativePath;

            return new SnapshotRecord(hash, date, size, path);
        }
    }
}