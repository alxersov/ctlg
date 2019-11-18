using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;
using Ctlg.Service.Utils;
using File = Ctlg.Core.File;

namespace Ctlg.Service.Services
{
    public class SnapshotService : ISnapshotService
    {
        public SnapshotService(IFilesystemService filesystemService)
        {
            FilesystemService = filesystemService;

            CurrentDirectory = FilesystemService.GetCurrentDirectory();

            CommentLineRegex = new Regex(@"^\s*#");
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

        public string FindSnapshotPath(string snapshotName, string snapshotDate = null)
        {
            return FindSnapshotFile(CurrentDirectory, snapshotName, snapshotDate)?.FullPath;
        }

        public SnapshotFile FindSnapshotFile(string rootPath, string snapshotName, string snapshotDate)
        {
            var snapshotDirectory = GetSnapshotDirectory(rootPath, snapshotName);
            var allSnapshots = GetSnapshotFiles(snapshotDirectory).Select(d => d.Name).OrderBy(s => s).ToList();

            if (allSnapshots.Count == 0)
            {
                return null;
            }

            var fileName = SelectSnapshotByDate(allSnapshots, snapshotDate);
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            string fullPath = FilesystemService.CombinePath(snapshotDirectory, fileName);

            return new SnapshotFile(snapshotName, fileName, fullPath);
        }

        public StreamWriter CreateSnapshotWriter(string name, string timestamp = null)
        {
            var snapshotDirectory = GetSnapshotDirectory(CurrentDirectory, name);
            FilesystemService.CreateDirectory(snapshotDirectory);

            var snapshotFileName = timestamp ?? GenerateSnapshotFileName();
            var snapshotPath = FilesystemService.CombinePath(snapshotDirectory, snapshotFileName);

            var snapshot = FilesystemService.CreateNewFileForWrite(snapshotPath);
            return new StreamWriter(snapshot);
        }

        public SnapshotRecord CreateSnapshotRecord(File file)
        {
            var hash = file.Hashes.First(h => h.HashAlgorithmId == (int)HashAlgorithmId.SHA256);
            var date = file.FileModifiedDateTime ?? DateTime.MinValue;
            var size = file.Size ?? 0;
            var path = file.RelativePath;

            return new SnapshotRecord(hash, date, size, path);
        }

        public File CreateFile(SnapshotRecord record)
        {
            var file = new File(record.Name)
            {
                FileModifiedDateTime = record.Date,
                Size = record.Size,
                RelativePath = record.Name
            };

            file.Hashes.Add(new Hash(HashAlgorithmId.SHA256, FormatBytes.ToByteArray(record.Hash)));

            return file;
        }

        private string CurrentDirectory { get; set; }

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
        private Regex CommentLineRegex { get; }
    }
}
