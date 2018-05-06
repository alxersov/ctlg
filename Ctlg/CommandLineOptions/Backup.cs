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

        [Option('f', "fast", HelpText = "Fast mode.", DefaultValue = false)]
        public bool Fast { get; set; }
    }
}
