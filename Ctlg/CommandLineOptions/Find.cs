using CommandLine;

namespace Ctlg.CommandLineOptions
{
    class Find
    {
        [Option('h', "hash", HelpText = "Hash value", Required = true)]
        public string Hash { get; set; }
    }
}
