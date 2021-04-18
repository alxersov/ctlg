using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Core.Utils;
using Ctlg.Service.Events;
using Ctlg.Service.Utils;
using File = Ctlg.Core.File;

namespace Ctlg.Service
{
    public class TextFileSnapshot: ISnapshot
    {
        public TextFileSnapshot(IFilesystemService filesystemService, HashAlgorithm hashAlgorithm,
            string snapshotFilePath, string name, string timestamp)
        {
            FilesystemService = filesystemService;
            HashAlgorithm = hashAlgorithm;
            SnapshotFilePath = snapshotFilePath;
            Name = name;
            Timestamp = timestamp;
        }

        public string Name { get; }
        public string Timestamp { get; }

        private IFilesystemService FilesystemService { get; }
        private string SnapshotFilePath { get; }
        private Regex CommentLineRegex { get; } = new Regex(@"^\s*#");
        private HashAlgorithm HashAlgorithm { get; }
        private Dictionary<string, SnapshotRecord> Records { get; set; }

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
                                snapshotRecord = CreateFile(line);
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
            // Workaround to prevent Sharing violation error when opening two zero-length files from exFAT drive when
            // running on Mono in macOS.
            // https://github.com/mono/mono/issues/19221
            PrepareSnapshotFile(SnapshotFilePath);

            var snapshot = FilesystemService.OpenFileForWrite(SnapshotFilePath);

            return new TextFileSnapshotWriter(new StreamWriter(snapshot), HashAlgorithm);
        }

        public SnapshotRecord CreateFile(string snapshotFileLine)
        {
            var match = BackupLineRegex.Match(snapshotFileLine);

            if (!match.Success)
            {
                throw new Exception($"Unexpected list line {snapshotFileLine}.");
            }

            var hash = FormatBytes.ToByteArray(match.Groups["hash"].Value);
            var size = long.Parse(match.Groups["size"].Value);
            var date = DateTime.ParseExact(match.Groups["date"].Value, "O", CultureInfo.InvariantCulture,
                                       DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            var name = match.Groups["name"].Value;

            var file = new SnapshotRecord
            {
                FileModifiedDateTime = date,
                Size = size,
                RelativePath = name,
                Hash = hash
            };

            return file;
        }

        public SnapshotRecord GetRecord(string relativePath)
        {
            if (Records == null) { LoadRecords(); }

            Records.TryGetValue(relativePath, out SnapshotRecord record);

            return record;
        }

        private void PrepareSnapshotFile(string path)
        {
            var stream = FilesystemService.CreateNewFileForWrite(SnapshotFilePath);
            using (var writer = new TextFileSnapshotWriter(new StreamWriter(stream), HashAlgorithm))
            {
                writer.AddComment($"ctlg {AppVersion.Version}");
            }
        }

        private void LoadRecords()
        {
            Records = new Dictionary<string, SnapshotRecord>();

            foreach (var record in EnumerateFiles())
            {
                Records.Add(record.RelativePath, record);
            }
        }

        private static Regex BackupLineRegex = new Regex(@"^(?<hash>[a-h0-9]{64,})\s(?<date>[0-9:.TZ-]{19,28})\s(?<size>[0-9]{1,10})\s(?<name>\S.*)$", RegexOptions.IgnoreCase);
    }
}
