using CommandLine;
using CommandLine.Text;

namespace Ctlg.CommandLineOptions
{
    class Options
    {
        [VerbOption("add", HelpText = "Add directory to the catalog.")]
        public Add Add { get; set; } = new Add();

        [VerbOption("find", HelpText = "Find file in the catalog.")]
        public Find Find { get; set; } = new Find();

        [VerbOption("list", HelpText = "List all files in the catalog.")]
        public object List { get; set; } = new object();

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            verb = verb?.ToLowerInvariant();

            var helpText = HelpText.AutoBuild(this, verb);

            string usage = null;

            switch (verb)
            {
                case null:
                    usage = "ctlg <command> [<args>]\n\nAvailable commands:";
                    break;
                case "add":
                    usage = "ctlg add <directory>";
                    break;
                case "find":
                    usage = "ctlg find [<options>]\n\nAvailable options:";
                    break;
                case "list":
                    usage = "ctlg list";
                    break;
            }

            if (usage != null)
            {
                helpText.AddPreOptionsLine("\nUsage: " + usage);
            }

            return helpText;
        }
    }
}
