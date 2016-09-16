using System.Collections.Generic;
using CommandLine;

namespace Ctlg.CommandLineOptions
{
    class Add
    {
        [ValueList(typeof(List<string>), MaximumElements = 1)]
        public IList<string> Path { get; set; }

        [Option('s', "search", HelpText = "Search pattern.")]
        public string SearchPattern { get; set; }

        [Option('f', "function", HelpText = "Hash function.")]
        public string HashFunctionName { get; set; }
    }
}
