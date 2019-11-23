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
        public BackupWriter(ICtlgService ctlgService,
            ISnapshotService snapshotService,
            IIndexService indexService,
            IFileStorageService fileStorageService,
            IHashFunction hashFunction,
            string name,
            string timestamp,
            bool shouldUseIndex,
            bool shouldExistingHashMatchCaclulated)
        {
            CtlgService = ctlgService;
            SnapshotService = snapshotService;
            IndexService = indexService;
            FileStorageService = fileStorageService;
            HashFunction = hashFunction;
            ShouldUseIndex = shouldUseIndex;
            ShouldExistingHashMatchCaclulated = shouldExistingHashMatchCaclulated;

            _streamWriter = snapshotService.CreateSnapshotWriter(name, timestamp);
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

        public void AddComment(string message)
        {
            _streamWriter.WriteLine($"# {message}");
        }

        private BackupFileStatus Process(File file)
        {
            var fileStatus = FindFile(file);
            if (fileStatus.IsNotFound())
            {
                if (fileStatus.HasFlag(BackupFileStatus.HashRecalculated))
                {
                    FileStorageService.AddFileToStorage(file);

                    var hash = file.Hashes.First(h => h.HashAlgorithmId == (int)HashAlgorithmId.SHA256);
                    IndexService.Add(hash.Value);
                }
                else
                {
                    var previousHash = file.Hashes.First(h => h.HashAlgorithmId == (int)HashAlgorithmId.SHA256);
                    file.Hashes.Clear();
                    fileStatus = Process(file);

                    if (ShouldExistingHashMatchCaclulated)
                    {
                        var calculatedHash = file.Hashes.First(h => h.HashAlgorithmId == (int)HashAlgorithmId.SHA256);

                        if (previousHash != calculatedHash)
                        {
                            throw new Exception($"Caclulated hash does not match expected for file {file.FullPath}.");
                        }
                    }
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
            else if (FileStorageService.IsFileInStorage(file))
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

        private ICtlgService CtlgService { get; }
        private ISnapshotService SnapshotService { get; }
        private IIndexService IndexService { get; }
        public IFileStorageService FileStorageService { get; }
        private IHashFunction HashFunction { get; }
        private bool ShouldUseIndex { get; }
        private bool ShouldExistingHashMatchCaclulated { get; }
    }
}
