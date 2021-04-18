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

        public void AddFile(File file, byte[] hash)
        {
            try
            {
                var fileStatus = FindFile(file, hash);
                if (fileStatus.IsNotFound())
                {
                    hash = HashCalculator.CalculateHashForFile(file).Value;
                    fileStatus = FindFile(file, hash) | BackupFileStatus.HashRecalculated;
                    if (fileStatus.IsNotFound())
                    {
                        Storage.AddFile(file, hash);
                        Index.Add(hash);
                    }
                }

                SnapshotWriter.AddFile(file, hash);

                DomainEvents.Raise(new BackupEntryCreated(file, hash,
                    fileStatus.HasFlag(BackupFileStatus.HashRecalculated),
                    fileStatus.HasFlag(BackupFileStatus.FoundInIndex),
                    fileStatus.IsNotFound()));
            }
            catch (Exception e)
            {
                DomainEvents.Raise(new ErrorEvent(e));
            }
        }

        public void AddFile(SnapshotRecord snapshotRecord, IFileStorage sourceStorage)
        {
            try
            {
                byte[] hash = null;
                var fileStatus = default(BackupFileStatus);

                if (sourceStorage.HashAlgorithmName == HashCalculator.Algorithm.Name)
                {
                    hash = snapshotRecord.Hash;
                    fileStatus = FindFile(snapshotRecord); // TODO: check if sourceStorage uses the same algorithm
                }

                if (fileStatus.IsNotFound())
                {
                    fileStatus = BackupFileStatus.HashRecalculated;
                    hash = Storage.AddFileFromStorage(snapshotRecord, sourceStorage);
                    Index.Add(hash);
                }

                var file = new File
                {
                    RelativePath = snapshotRecord.RelativePath,
                    Size = snapshotRecord.Size,
                    FileModifiedDateTime = snapshotRecord.FileModifiedDateTime
                };

                SnapshotWriter.AddFile(file, hash);

                DomainEvents.Raise(new BackupEntryCreated(file, hash,
                    fileStatus.HasFlag(BackupFileStatus.HashRecalculated),
                    fileStatus.HasFlag(BackupFileStatus.FoundInIndex),
                    fileStatus.IsNotFound()));
            }
            catch (Exception e)
            {
                DomainEvents.Raise(new ErrorEvent(e));
            }
        }

        protected BackupFileStatus FindFile(SnapshotRecord snapshotRecord)
        {
            return FindFile(new File { Name = snapshotRecord.RelativePath, Size = snapshotRecord.Size }, snapshotRecord.Hash);
        }

        protected BackupFileStatus FindFile(File file, byte[] hash)
        {
            var status = default(BackupFileStatus);

            if (hash == null)
            {
                return status;
            }

            if (ShouldUseIndex && Index.IsInIndex(hash))
            {
                status |= BackupFileStatus.FoundInIndex;
            }
            else if (Storage.IsFileInStorage(file, hash))
            {
                status |= BackupFileStatus.FoundInStorage;
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
        public ISnapshot PreviousSnapshot { get; }
        private bool ShouldUseIndex { get; }
        private IFileStorageIndex Index { get; }
        private HashCalculator HashCalculator { get; }
    }
}
