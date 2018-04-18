using System;
using CommandLine;

namespace Ctlg.CommandLineOptions
{
    class Backup
    {
        [ValueOption(0)]
        public string Path { get; set; }

        [Option('n', "name", HelpText = "Backup name.")]
        public string Name { get; set; }
    }
}
