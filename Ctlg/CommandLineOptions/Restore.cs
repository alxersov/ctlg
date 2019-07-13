using System;
using CommandLine;

namespace Ctlg.CommandLineOptions
{
    [Verb("restore", HelpText = "Restore directory from backup.")]
    class Restore
    {
        [Value(0)]
        public string Path { get; set; }

        [Option('n', "name", HelpText = "Backup name.")]
        public string Name { get; set; }

        [Option('d', "date", HelpText = "Backup date.")]
        public string Date { get; set; }
    }
}
