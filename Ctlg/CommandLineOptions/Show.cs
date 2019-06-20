using System;
using System.Collections.Generic;
using CommandLine;

namespace Ctlg.CommandLineOptions
{
    [Verb("show", HelpText = "Show catalog entry.")]
    class Show
    {
        [Value(0)]
        public IList<string> CatalogEntryIds { get; set; }
    }
}
