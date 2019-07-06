using System;
using Autofac.Features.Indexed;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Events;

namespace Ctlg.Service.Commands
{
    public class AddCommand : ICommand
    {
        public string HashFunctionName { get; set; }
        public string Path { get; set; }
        public string SearchPattern { get; set; }

        private IHashFunction HashFunction;

        private ITreeProvider TreeProvider { get; }
        private IIndex<string, IHashFunction> HashFunctions { get; }
        private IDataService DataService { get; }
        private IFilesystemService FilesystemService { get; }

        public AddCommand(ITreeProvider treeProvider, IIndex<string, IHashFunction> hashFunctions,
            IDataService dataService, IFilesystemService filesystemService)
        {
            DataService = dataService;
            FilesystemService = filesystemService;
            TreeProvider = treeProvider;
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

            var root = TreeProvider.ReadTree(Path, SearchPattern);
            var treeWalker = new TreeWalker(root);
            treeWalker.Walk(ProcessFile);


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
                DomainEvents.Raise(new ErrorEvent(e));
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
                DomainEvents.Raise(new ErrorEvent(e));
            }
        }

        protected void ProcessFile(File file)
        {
            CalculateHashes(file);

            if (FilesystemService.IsArchiveExtension(file.FullPath))
            {
                ProcessArchive(file);
            }
        }
    }
}
