using System.Collections.Generic;
using CommandLine;

namespace Ctlg.CommandLineOptions
{
    [Verb("add", HelpText="Add directory to the catalog.")]
    class Add
    {
        [Value(0, MetaName = "Path")]
        public IList<string> Path { get; set; }

        [Option('s', "search", HelpText = "Search pattern. Can contain wildcards * and ?.")]
        public string SearchPattern { get; set; }

        [Option('f', "function", HelpText = "Hash function.")]
        public string HashFunctionName { get; set; }
    }
}
