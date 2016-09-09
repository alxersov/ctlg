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
        IHandle<HashCalculated>,
        IHandle<TreeItemEnumerated>,
        IHandle<AddCommandFinished>,
        IHandle<FileFoundInDb>
    {
        public ConsoleOutput()
        {
            _filesFound = 0;
        }


        public void Handle(DirectoryFound args)
        {
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
            Console.WriteLine("{0} files found.",_filesFound);
        }

        private int _filesFound;
        public void Handle(FileFoundInDb args)
        {
            var f = args.File;
            Console.WriteLine("{0} {1}", f.BuildFullPath(), f.RecordUpdatedDateTime);
        }
    }
}
