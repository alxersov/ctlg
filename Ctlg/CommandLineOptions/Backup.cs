using System;
using CommandLine;

namespace Ctlg.CommandLineOptions
{
    [Verb("backup", HelpText = "Backup directory.")]
    class Backup
    {
        [Value(0)]
        public string Path { get; set; }

        [Option('n', "name", HelpText = "Backup name.")]
        public string Name { get; set; }

        [Option('f', "fast", HelpText = "Fast mode.", Default = false)]
        public bool Fast { get; set; }
    }
}
