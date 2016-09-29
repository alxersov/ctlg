using System;
using System.Linq;
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
        IHandle<FileFoundInDb>
    {
        public void Handle(DirectoryFound args)
        {
            ++_directoriesFound;
            using (new ConsoleTextAttributesScope())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(args.FullPath);
            }
        }

        public void Handle(FileFound args)
        {
            ++_filesFound;
            Console.WriteLine(args.FullPath);
        }

        public void Handle(ArchiveFound args)
        {
            ++_archivesFound;
            Console.Write("Archive: ");
            Console.WriteLine(args.FullPath);
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
                args.FullPath);
        }

        public void Handle(TreeItemEnumerated args)
        {
            var padding = "".PadLeft(args.NestingLevel * 4 + 1);
            var hashes = string.Join(" ", args.File.Hashes.Select(h => FormatBytes.ToHexString(h.Value)));

            if (string.IsNullOrEmpty(hashes))
            {
                hashes = "".PadLeft(40);
            }

            using (new ConsoleTextAttributesScope())
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(hashes);
            }

            Console.WriteLine("{0} {1}", padding, args.File.Name);
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
            Console.WriteLine("{0} {1}", f.BuildFullPath(), f.RecordUpdatedDateTime);
        }

        private int _filesFound = 0;
        private int _directoriesFound = 0;
        private int _archivesFound = 0;
    }
}
