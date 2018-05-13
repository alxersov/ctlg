using CommandLine;

namespace Ctlg.CommandLineOptions
{
    [Verb("find", HelpText = "Find file in the catalog.")]
    class Find
    {
        [Option('f', "function", HelpText = "Hash function.")]
        public string HashFunctionName { get; set; }

        [Option('c', "checksum", HelpText = "Checksum value.")]
        public string Checksum { get; set; }

        [Option('s', "size", HelpText = "File size in bytes.")]
        public long? Size { get; set; }

        [Option('n', "name", HelpText = "Name pattern. Can contain wildcards * and ?.")]
        public string NamePattern { get; set; }
    }
}
