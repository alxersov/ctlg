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
    public class BackupWriter: IBackupWriter
    {
        public BackupWriter(StreamWriter writer,
            IFilesystemService filesystemService,
            ICtlgService ctlgService,
            ISnapshotService snapshotService,
            IIndexService indexService,
            IHashFunction hashFunction,
            bool shouldUseIndex)
        {
            _streamWriter = writer;
            FilesystemService = filesystemService;
            CtlgService = ctlgService;
            SnapshotService = snapshotService;
            IndexService = indexService;
            HashFunction = hashFunction;
            ShouldUseIndex = shouldUseIndex;
        }

        public void AddFile(File file)
        {
            try
            {
                var fileStatus = Process(file);

                var snapshotRecord = SnapshotService.CreateSnapshotRecord(file);

                _streamWriter.WriteLine(snapshotRecord);

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

        private BackupFileStatus Process(File file)
        {
            var fileStatus = FindFile(file);
            if (fileStatus.IsNotFound())
            {
                if (fileStatus.HasFlag(BackupFileStatus.HashRecalculated))
                {
                    CtlgService.AddFileToStorage(file);
                }
                else
                {
                    file.Hashes.Clear();
                    return Process(file);
                }
            }

            return fileStatus;
        }

        private Hash GetHashForFile(File file, out bool hashCalculated)
        {
            var hash = GetExistingHashValue(file);
            if (hash != null)
            {
                hashCalculated = false;
                return hash;
            }
            else
            {
                hashCalculated = true;
                return CtlgService.CalculateHashForFile(file, HashFunction);
            }
        }

        private BackupFileStatus FindFile(File file)
        {
            var status = default(BackupFileStatus);

            var hash = GetHashForFile(file, out bool isHashCalculated);
            if (isHashCalculated)
            {
                status |= BackupFileStatus.HashRecalculated;
            }
 
            if (ShouldUseIndex && IndexService.IsInIndex(hash.Value))
            {
                status |= BackupFileStatus.FoundInIndex;
            }
            else if (CtlgService.IsFileInStorage(file))
            {
                status |= BackupFileStatus.FoundInStorage;
            }

            return status;
        }

        private static Hash GetExistingHashValue(File file)
        {
            return file.Hashes.FirstOrDefault(h => h.HashAlgorithmId == (int)HashAlgorithmId.SHA256);
        }

        public void Dispose()
        {
            _streamWriter.Dispose();
        }

        private StreamWriter _streamWriter;

        public IFilesystemService FilesystemService { get; }
        public ICtlgService CtlgService { get; }
        public ISnapshotService SnapshotService { get; }
        public IIndexService IndexService { get; }
        public IHashFunction HashFunction { get; }
        public bool ShouldUseIndex { get; }
    }
}
