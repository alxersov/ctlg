using System;
using Ctlg.Core;

namespace Ctlg.UnitTests
{
    public abstract class BackupTestFixture: BaseTestFixture
    {
        protected readonly string BackupName;
        protected readonly string SnapshotPath;
        protected readonly string Hash;
        protected readonly string FilePath;
        protected readonly string FileListLine;
        protected readonly string BackupDirectory;
        protected readonly string BackupFileName;
        protected readonly string SourcePath;
        protected readonly string SourceFilePath;
        protected readonly string RestorePath;
        protected readonly SnapshotRecord SnapshotRecordSample;

        public BackupTestFixture()
        {
            BackupName = "MyBackup";
            SnapshotPath = @"snapshots\MyBackup\2019-01-01_02-30-00";
            Hash = "64ec88ca00b268e5ba1a35678a1b5316d212f4f366b2477232534a8aeca37f3c";
            FilePath = @"1.txt";
            FileListLine = $"{Hash} 2018-04-22T18:05:12.0000000Z 11 {FilePath}";
            BackupDirectory = $@"foo\64";
            BackupFileName = $@"{BackupDirectory}\{Hash}";
            SourcePath = @"C:\foo";
            SourceFilePath = $@"{SourcePath}\{FilePath}";
            RestorePath = @"C:\foo\restore";
            SnapshotRecordSample = new SnapshotRecord(FileListLine);
        }
    }
}
