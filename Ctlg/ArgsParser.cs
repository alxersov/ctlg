using System;
using Ctlg.Service.Commands;

namespace Ctlg
{
    public class ArgsParser
    {
        public ICommand Parse(string[] args)
        {
            ICommand command = null;

            if (args.Length == 2 && args[0].Equals("add", StringComparison.OrdinalIgnoreCase))
            {
                command = new AddCommand
                {
                    Path = args[1]
                };
            }

            return command;
        }
    }
}
