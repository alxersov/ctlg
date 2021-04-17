using System;
using Ctlg.Core;
using Ctlg.Service;
using Ctlg.Service.Events;
using Ctlg.Service.Utils;

namespace Ctlg.EventHandlers
{
    public class ConsoleOutput :
        IHandle<FileFound>,
        IHandle<DirectoryFound>,
        IHandle<BackupEntryCreated>,
        IHandle<BackupEntryRestored>,
        IHandle<BackupCommandEnded>,
        IHandle<Warning>,
        IHandle<EnumeratingHashes>,
        IHandle<EnumeratingSnapshots>
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

        public void Handle(BackupEntryCreated args)
        {
            ++_filesProcessed;
            bytesProcessed += args.File.Size ?? 0;
            if (args.NewFileAddedToStorage)
            {
                bytesAddedToStorage += args.File.Size ?? 0;
            }

            var h = args.HashCalculated ? 'H' : ' ';
            var n = args.IsHashFoundInIndex ? 'I' : args.NewFileAddedToStorage ? 'N' : ' ';

            var maxCounterLength = _filesFound.ToString().Length;
            var counter = _filesProcessed.ToString().PadLeft(maxCounterLength);

            var filesFound = 0 < _filesFound ? $"/{_filesFound}" : "";

            Console.WriteLine($"{counter}{filesFound} {h}{n} {FormatFileInfo(args.File, args.Hash)}");
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

        private string FormatFileInfo(File file, byte[] hash)
        {
            return $"{FormatBytes.ToHexString(hash).Substring(0, 8)} {FileSize.Format(file.Size ?? 0),6} {file.Name}";
        }

        public void Handle(EnumeratingHashes args)
        {
            Console.WriteLine($"Enumerating files in storage with {args.Prefix}.. hashes.");
        }

        public void Handle(EnumeratingSnapshots args)
        {
            Console.WriteLine($"Processing snapshot {args.Name} @ {args.Timestamp}");
        }

        private int _filesFound = 0;
        private int _filesProcessed = 0;
        private int _directoriesFound = 0;
        private long bytesProcessed = 0;
        private long bytesAddedToStorage = 0;
    }
}
