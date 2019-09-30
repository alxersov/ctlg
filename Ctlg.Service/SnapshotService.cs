using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Autofac.Features.Indexed;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;
using File = Ctlg.Core.File;

namespace Ctlg.Service
{
    public class SnapshotService: ISnapshotService
    {
        public SnapshotService(IFilesystemService filesystemService, ICtlgService ctlgService)
        {
            FilesystemService = filesystemService;
            CtlgService = ctlgService;

            var currentDirectory = FilesystemService.GetCurrentDirectory();
            SnapshotsDirectory = FilesystemService.CombinePath(currentDirectory, "snapshots");
        }

        public IEnumerable<File> GetSnapshotFiles(string snapshotName)
        {
            var snapshotsPath = GetSnapshotDirectory(snapshotName);
            if (!FilesystemService.DirectoryExists(snapshotsPath))
            {
                return Enumerable.Empty<File>();
            }
            return FilesystemService.EnumerateFiles(snapshotsPath, "????-??-??_??-??-??");
        }

        public IEnumerable<SnapshotRecord> ReadSnapshotFile(string path)
        {
            using (var stream = FilesystemService.OpenFileForRead(path))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var line = reader.ReadLine();
                    while (line != null)
                    {
                        SnapshotRecord snapshotRecord = null;
                        try
                        {
                            snapshotRecord = new SnapshotRecord(line);
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

        public string FindSnapshotPath(string snapshotName, string snapshotDate = null)
        {
            var allSnapshots = GetSnapshotFiles(snapshotName).Select(d => d.Name).OrderBy(s => s).ToList();

            if (allSnapshots.Count == 0)
            {
                return null;
            }

            var fileName = SelectSnapshotByDate(allSnapshots, snapshotDate);
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            return FilesystemService.CombinePath(SnapshotsDirectory, snapshotName, fileName);
        }

        public ISnapshotWriter CreateSnapshotWriter(string name)
        {
            var hashFunction = CtlgService.GetHashFunction("SHA-256");

            var backupDirectory = GetSnapshotDirectory(name);
            FilesystemService.CreateDirectory(backupDirectory);

            var snapshotName = GenerateSnapshotFileName();
            var snapshotPath = FilesystemService.CombinePath(backupDirectory, snapshotName);

            var fileList = FilesystemService.CreateNewFileForWrite(snapshotPath);
            var fileListWriter = new StreamWriter(fileList);
            return new SnapshotWriter(fileListWriter, FilesystemService, CtlgService, hashFunction);
        }

        private string SnapshotsDirectory { get; set; }

        private string GetSnapshotDirectory(string snapshotName)
        {
            return FilesystemService.CombinePath(SnapshotsDirectory, snapshotName);
        }

        private string GenerateSnapshotFileName()
        {
            return FormatSnapshotName(DateTime.UtcNow);
        }

        private string FormatSnapshotName(DateTime date)
        {
            return date.ToString("yyyy-MM-dd_HH-mm-ss");
        }

        private string SelectSnapshotByDate(IEnumerable<string> snapshots, string snapshotDate)
        {
            if (string.IsNullOrEmpty(snapshotDate))
            {
                return snapshots.Last();
            }
            else
            {
                var foundFiles = snapshots.Where(s => s.StartsWith(snapshotDate, StringComparison.InvariantCultureIgnoreCase)).ToList();
                if (foundFiles.Count > 1)
                {
                    throw new Exception(
                        $"Provided snapshot date is ambiguous. {foundFiles.Count} snapshots exist: {string.Join(", ", foundFiles)}.");
                }
                return foundFiles.FirstOrDefault();
            }
        }

        private IFilesystemService FilesystemService { get; }
        private ICtlgService CtlgService { get; }
    }
}
