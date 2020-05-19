using System;
using System.Collections.Generic;
using System.Linq;
using Ctlg.Core.Interfaces;
using File = Ctlg.Core.File;

namespace Ctlg.Service.Services
{
    public class SnapshotService : ISnapshotService
    {
        public SnapshotService(IFilesystemService filesystemService, IDataService dataService)
        {
            FilesystemService = filesystemService;
            DataService = dataService;
        }

        public ISnapshot GetSnapshot(string backupRootPath, string hashAlgorithmName, string name, string timestampMask)
        {
            var snapshotDirectory = GetSnapshotDirectory(backupRootPath, name);
            var allSnapshots = GetSnapshotFiles(snapshotDirectory).Select(d => d.Name).OrderBy(s => s).ToList();

            if (allSnapshots.Count == 0)
            {
                return null;
            }

            var timestamp = SelectSnapshotByDate(allSnapshots, timestampMask);
            if (string.IsNullOrEmpty(timestamp))
            {
                return null;
            }

            string fullPath = FilesystemService.CombinePath(snapshotDirectory, timestamp);
            var hashAlgorithm = DataService.GetHashAlgorithm(hashAlgorithmName);

            return new TextFileSnapshot(FilesystemService, hashAlgorithm, fullPath, name, timestamp);
        }

        public ISnapshot CreateSnapshot(string backupRootPath, string hashAlgorithmName, string name, string timestamp)
        {
            var snapshotDirectory = GetSnapshotDirectory(backupRootPath, name);
            FilesystemService.CreateDirectory(snapshotDirectory);

            var snapshotFileName = timestamp ?? GenerateSnapshotFileName();
            var fullPath = FilesystemService.CombinePath(snapshotDirectory, snapshotFileName);
            var hashAlgorithm = DataService.GetHashAlgorithm(hashAlgorithmName);

            return new TextFileSnapshot(FilesystemService, hashAlgorithm, fullPath, name, snapshotFileName);
        }

        private string GetSnapshotDirectory(string root, string snapshotName)
        {
            return FilesystemService.CombinePath(root, "snapshots", snapshotName);
        }

        private IEnumerable<File> GetSnapshotFiles(string snapshotDirectory)
        {
            if (!FilesystemService.DirectoryExists(snapshotDirectory))
            {
                return Enumerable.Empty<File>();
            }
            return FilesystemService.EnumerateFiles(snapshotDirectory, "????-??-??_??-??-??");
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

            var foundFiles = snapshots.Where(s => s.StartsWith(snapshotDate, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (foundFiles.Count > 1)
            {
                    throw new Exception(
                    $"Provided snapshot date is ambiguous. {foundFiles.Count} snapshots exist: {string.Join(", ", foundFiles)}.");
            }
            return foundFiles.FirstOrDefault();
        }

        private IFilesystemService FilesystemService { get; }
        private IDataService DataService { get; }
    }
}
