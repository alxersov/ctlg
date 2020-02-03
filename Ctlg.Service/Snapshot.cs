using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;

namespace Ctlg.Service
{
    public class Snapshot: ISnapshot
    {
        public Snapshot(IFilesystemService filesystemService,
            string snapshotFilePath, string name, string timestamp)
        {
            FilesystemService = filesystemService;
            SnapshotFilePath = snapshotFilePath;
            Name = name;
            Timestamp = timestamp;

            CommentLineRegex = new Regex(@"^\s*#");
        }

        private IFilesystemService FilesystemService { get; }
        private string SnapshotFilePath { get; }
        public string Name { get; }

        public string Timestamp { get; }

        private Regex CommentLineRegex { get; }

        public IEnumerable<SnapshotRecord> EnumerateFiles()
        {
            using (var stream = FilesystemService.OpenFileForRead(SnapshotFilePath))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var line = reader.ReadLine();
                    while (line != null)
                    {
                        SnapshotRecord snapshotRecord = null;
                        try
                        {
                            if (!CommentLineRegex.IsMatch(line))
                            {
                                snapshotRecord = new SnapshotRecord(line);
                            }
                        }
                        catch (Exception ex)
                        {
                            DomainEvents.Raise(new ErrorEvent(ex));
                        }

                        if (snapshotRecord != null)
                        {
                            yield return snapshotRecord;
                        }

                        line = reader.ReadLine();
                    }
                }
            }
        }

        public ISnapshotWriter GetWriter()
        {
            var snapshot = FilesystemService.CreateNewFileForWrite(SnapshotFilePath); 

            return new SnapshotWriter(new StreamWriter(snapshot));
        }
    }
}
