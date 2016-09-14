using System.Collections.Generic;
using CommandLine;

namespace Ctlg.CommandLineOptions
{
    class Add
    {
        [ValueList(typeof(List<string>), MaximumElements = 1)]
        public IList<string> Path { get; set; }
    }
}
