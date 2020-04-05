using System;
using System.Linq;
using Ctlg.Core;
using Ctlg.Service;
using Ctlg.Service.Events;
using Ctlg.Service.Utils;

namespace Ctlg.EventHandlers
{
    public class ConsoleOutput :
        IHandle<FileFound>,
        IHandle<DirectoryFound>,
        IHandle<ArchiveFound>,
        IHandle<ArchiveEntryFound>,
        IHandle<HashCalculated>,
        IHandle<TreeItemEnumerated>,
        IHandle<AddCommandFinished>,
        IHandle<FileFoundInDb>,
        IHandle<CatalogEntryNotFound>,
        IHandle<CatalogEntryFound>,
        IHandle<BackupEntryCreated>,
        IHandle<BackupEntryRestored>,
        IHandle<BackupCommandEnded>,
        IHandle<Warning>
    {
        public void Handle(DirectoryFound args)
        {
            ++_directoriesFound;
            using (new ConsoleTextAttributesScope())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                if (!string.IsNullOrEmpty(args.Path))
                {
                    Console.WriteLine(args.Path);
                }
            }
        }

        public void Handle(FileFound args)
        {
            ++_filesFound;
            Console.WriteLine(args.Path);
        }

        public void Handle(ArchiveFound args)
        {
            ++_archivesFound;
            Console.Write("Archive: ");
            Console.WriteLine(args.Path);
        }

        public void Handle(ArchiveEntryFound args)
        {
            using (new ConsoleTextAttributesScope())
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(args.EntryKey);
            }
        }

        public void Handle(HashCalculated args)
        {
            Console.WriteLine("{0} {1}",
                FormatBytes.ToHexString(args.Hash),
                args.Path);
        }

        public void Handle(TreeItemEnumerated args)
        {
            var padding = "".PadLeft(args.NestingLevel * 2);
            Console.WriteLine("{0}{1}: {2}", padding, args.File.FileId, args.File.Name);
        }

        public void Handle(AddCommandFinished args)
        {
            Console.WriteLine("{0} directories processed.",_directoriesFound);
            Console.WriteLine("{0} archives found.",_archivesFound);
            Console.WriteLine("{0} files found.",_filesFound);
        }

        public void Handle(FileFoundInDb args)
        {
            var f = args.File;
            Console.WriteLine("{0}: {1} {2}", f.FileId, f.BuildFullPath(), f.RecordUpdatedDateTime);
        }

        public void Handle(CatalogEntryNotFound args)
        {
            Console.WriteLine("Entry with ID {0} not found.", args.Id);
        }

        public void Handle(CatalogEntryFound args)
        {
            var e = args.Entry;
            Console.WriteLine("ID: {0}", e.FileId);
            Console.WriteLine("Name: {0}", e.Name);
            Console.WriteLine("Path: {0}", e.BuildFullPath());
            if (e.Size.HasValue) { Console.WriteLine("Size: {0} bytes", e.Size); }
            if (e.FileCreatedDateTime.HasValue) { Console.WriteLine("Created: {0}", e.FileCreatedDateTime); }
            if (e.FileModifiedDateTime.HasValue) { Console.WriteLine("Modified: {0}", e.FileModifiedDateTime); }
            Console.WriteLine("Entry updated: {0}", e.RecordUpdatedDateTime);
            if (e.IsDirectory) { Console.WriteLine("Is directory"); }

            foreach (var hash in e.Hashes)
            {
                Console.WriteLine("{0}: {1}", hash.HashAlgorithm.Name, FormatBytes.ToHexString(hash.Value));
            }

            if (e.ParentFile != null)
            {
                Console.WriteLine("Parent:");

                Console.WriteLine(" ^ {0}: {1}", e.ParentFile.FileId, e.ParentFile.BuildFullPath());
            }

            if (e.Contents.Any())
            {
                Console.WriteLine("Contents:");

                foreach (var content in e.Contents)
                {
                    Console.WriteLine(" > {0}: {1}", content.FileId, content.Name);
                }
            }


            Console.WriteLine();
        }

        public void Handle(BackupEntryCreated args)
        {
            ++_filesProcessed;
            bytesProcessed += args.BackupEntry.Size;
            if (args.NewFileAddedToStorage)
            {
                bytesAddedToStorage += args.BackupEntry.Size;
            }

            var h = args.HashCalculated ? 'H' : ' ';
            var n = args.IsHashFoundInIndex ? 'I' : args.NewFileAddedToStorage ? 'N' : ' ';

            var maxCounterLength = _filesFound.ToString().Length;
            var counter = _filesProcessed.ToString().PadLeft(maxCounterLength);

            var filesFound = 0 < _filesFound ? $"/{_filesFound}" : "";

            Console.WriteLine($"{counter}{filesFound} {h}{n} {FormatSnapshotRecord(args.BackupEntry)}");
        }

        public void Handle(BackupEntryRestored args)
        {
            ++_filesProcessed;

            Console.WriteLine($"{_filesProcessed} {args.BackupEntry}");
        }

        public void Handle(BackupCommandEnded args)
        {
            Console.WriteLine($"Processed: {FileSize.Format(bytesProcessed)}B");
            Console.WriteLine($"Added to storage: {FileSize.Format(bytesAddedToStorage)}B");
        }

        public void Handle(Warning args)
        {
            using (new ConsoleTextAttributesScope())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(args.Message);
            }
        }

        private string FormatSnapshotRecord(SnapshotRecord record)
        {
            return $"{record.Hash.ToString().Substring(0, 8)} {FileSize.Format(record.Size),6} {record.Name}";
        }

        private int _filesFound = 0;
        private int _filesProcessed = 0;
        private int _directoriesFound = 0;
        private int _archivesFound = 0;
        private long bytesProcessed = 0;
        private long bytesAddedToStorage = 0;
    }
}
