using System;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;
using Ctlg.Service.Utils;
using File = Ctlg.Core.File;

namespace Ctlg.Service
{
    public class BackupWriter : IBackupWriter
    {
        public BackupWriter(IFileStorage fileStorage,
            ISnapshotWriter snapshotWriter,
            bool shouldUseIndex,
            IFileStorageIndex index,
            HashCalculator hashCalculator)
        {
            Storage = fileStorage;
            SnapshotWriter = snapshotWriter;
            Index = index;
            ShouldUseIndex = shouldUseIndex;
            HashCalculator = hashCalculator;

            index.Load();
        }

        public void AddFile(File file, IFileStorage sourceStorage = null)
        {
            try
            {
                var fileStatus = FindFile(file);
                if (fileStatus.IsNotFound())
                {
                    fileStatus = sourceStorage == null ?
                        AddFileFromFilesystem(file) :
                        AddFileFromStorage(file, sourceStorage);

                    if (fileStatus.IsNotFound())
                    {
                        Index.Add(HashCalculator.GetExistingHashValue(file).Value);
                    }
                }

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

        protected BackupFileStatus AddFileFromFilesystem(File file)
        {
            file.Hashes.Clear();
            HashCalculator.CalculateHashForFile(file);
            var fileStatus = FindFile(file) | BackupFileStatus.HashRecalculated;

            if (fileStatus.IsNotFound())
            {
                Storage.AddFile(file);
            }

            return fileStatus;
        }

        protected BackupFileStatus AddFileFromStorage(File file, IFileStorage storage)
        {
            Storage.AddFileFromStorage(file, storage);

            return BackupFileStatus.HashRecalculated;
        }

        protected BackupFileStatus FindFile(File file)
        {
            var status = default(BackupFileStatus);

            var hash = HashCalculator.GetExistingHashValue(file);
            if (hash != null)
            {
                if (ShouldUseIndex && Index.IsInIndex(hash.Value))
                {
                    status |= BackupFileStatus.FoundInIndex;
                }
                else if (Storage.IsFileInStorage(file))
                {
                    status |= BackupFileStatus.FoundInStorage;
                }
            }

            return status;
        }

        public void AddComment(string message)
        {
            SnapshotWriter.AddComment(message);
        }

        public void Dispose()
        {
            Index.Save();
            SnapshotWriter.Dispose();
        }

        private IFileStorage Storage { get; }
        private ISnapshotWriter SnapshotWriter { get; }
        private bool ShouldUseIndex { get; }
        private IFileStorageIndex Index { get; }
        private HashCalculator HashCalculator { get; }
    }
}
