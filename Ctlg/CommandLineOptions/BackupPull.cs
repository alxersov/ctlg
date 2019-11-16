using System;
using CommandLine;

namespace Ctlg.CommandLineOptions
{
    [Verb("pull-backup", HelpText = "Imports snapshot from another storage.")]
    public class BackupPull
    {
        [Value(0)]
        public string Path { get; set; }

        [Option('n', "name", HelpText = "Backup name.")]
        public string Name { get; set; }

        [Option('d', "date", HelpText = "Backup date.")]
        public string Date { get; set; }
    }
}
