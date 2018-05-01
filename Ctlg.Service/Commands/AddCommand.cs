using System;
using Autofac.Features.Indexed;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;

namespace Ctlg.Service.Commands
{
    public class AddCommand : TreeProcessingCommand, ICommand
    {
        public string HashFunctionName { get; set; }

        private IHashFunction HashFunction;
        private IIndex<string, IHashFunction> HashFunctions { get; }
        private IDataService DataService { get; }

        public AddCommand(IIndex<string, IHashFunction> hashFunctions, IDataService dataService, IFilesystemService filesystemService): base(filesystemService)
        {
            DataService = dataService;
            HashFunctions = hashFunctions;
        }

        public void Execute(ICtlgService svc)
        {
            var hashFunctionName = HashFunctionName ?? "SHA-1";
            hashFunctionName = hashFunctionName.ToUpperInvariant();

            if (!HashFunctions.TryGetValue(hashFunctionName, out HashFunction))
            {
                throw new Exception($"Unsupported hash function {hashFunctionName}");
            }

            var root = ReadTree();

            ProcessTree(root);

            DataService.AddDirectory(root);

            DataService.SaveChanges();

            DomainEvents.Raise(new AddCommandFinished());
        }

        private void CalculateHashes(File file)
        {
            try
            {
                using (var stream = FilesystemService.OpenFileForRead(file.FullPath))
                {
                    var hash = HashFunction.CalculateHash(stream);

                    DomainEvents.Raise(new HashCalculated(file.RelativePath, hash.Value));

                    file.Hashes.Add(hash);
                }
            }
            catch (Exception e)
            {
                DomainEvents.Raise(new ExceptionEvent(e));
            }
        }

        private void ProcessArchive(File file)
        {
            try
            {
                using (var stream = FilesystemService.OpenFileForRead(file.FullPath))
                {
                    var archive = FilesystemService.OpenArchive(stream);

                    DomainEvents.Raise(new ArchiveFound(file.FullPath));

                    foreach (var entry in archive.EnumerateEntries())
                    {
                        file.Contents.Add(entry);

                        DomainEvents.Raise(new ArchiveEntryFound(entry.Name));
                    }
                }
            }
            catch (Exception e)
            {
                DomainEvents.Raise(new ExceptionEvent(e));
            }
        }

        protected override void ProcessFile(File file)
        {
            CalculateHashes(file);

            if (FilesystemService.IsArchiveExtension(file.FullPath))
            {
                ProcessArchive(file);
            }
        }
    }
}
