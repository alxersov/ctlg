using System;
using System.Collections.Generic;
using CommandLine;

namespace Ctlg.CommandLineOptions
{
    class Show
    {
        [ValueList(typeof(List<string>))]
        public IList<string> CatalogEntryIds { get; set; }
    }
}
