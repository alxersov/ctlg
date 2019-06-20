using System;
using System.IO;
using Autofac.Features.Indexed;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Commands
{
    public class SnapshotWriterProvider: ISnapshotWriterProvider
    {
        public SnapshotWriterProvider(IFilesystemService filesystemService, ICtlgService ctlgService, IIndex<string, IHashFunction> hashFunctions)
        {
            FilesystemService = filesystemService;
            CtlgService = ctlgService;
            HashFunctions = hashFunctions;
        }


        public ISnapshotWriter CreateSnapshotWriter(string name)
        {
            var hashFunctionName = "SHA-256";
            if (!HashFunctions.TryGetValue(hashFunctionName, out IHashFunction hashFunction))
            {
                throw new Exception($"Unsupported hash function {hashFunctionName}");
            }

            var backupDirectory = CtlgService.GetBackupSnapshotDirectory(name);
            FilesystemService.CreateDirectory(backupDirectory);

            var snapshotName = CtlgService.GenerateSnapshotFileName();
            var snapshotPath = FilesystemService.CombinePath(backupDirectory, snapshotName);

            var fileList = FilesystemService.CreateNewFileForWrite(snapshotPath);
            var fileListWriter = new StreamWriter(fileList);
            return new SnapshotWriter(fileListWriter, FilesystemService, CtlgService, hashFunction);
        }

        private IFilesystemService FilesystemService { get; }
        public ICtlgService CtlgService { get; }
        public IIndex<string, IHashFunction> HashFunctions { get; }
    }
}
