using System;
using Ctlg.Service;
using Ctlg.Service.Events;

namespace Ctlg.EventHandlers
{
    public class ErrorOutput: IHandle<ExceptionEvent>
    {
        public void Handle(ExceptionEvent args)
        {
            using (new ConsoleTextAttributesScope())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(args.Exception.Message);
            }
        }
    }
}
