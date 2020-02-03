using System;
using System.IO;
using System.Linq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;
using Ctlg.Service.Utils;
using File = Ctlg.Core.File;

namespace Ctlg.Service
{
    public sealed class BackupWriter: IBackupWriter
    {
        public BackupWriter(
            IFileStorage fileStorage,
            ISnapshotWriter snapshotWriter)
        {
            FileStorage = fileStorage;
            SnapshotWriter = snapshotWriter;
        }

        public void AddFile(File file)
        {
            try
            {
                var fileStatus = FileStorage.AddFileToStorage(file);

                var snapshotRecord = SnapshotWriter.AddFile(file);

                DomainEvents.Raise(new BackupEntryCreated(snapshotRecord,
                    fileStatus.HasFlag(BackupFileStatus.HashRecalculated),
                    fileStatus.HasFlag(BackupFileStatus.FoundInIndex),
                    fileStatus.IsNotFound()));
            }
            catch (Exception e)
            {
                DomainEvents.Raise(new ErrorEvent(e));
            }
        }

        public IFileStorage FileStorage { get; }
        public ISnapshotWriter SnapshotWriter { get; }
    }
}
