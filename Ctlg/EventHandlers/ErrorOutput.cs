using System;
using Ctlg.Service;
using Ctlg.Service.Events;

namespace Ctlg.EventHandlers
{
    public class ErrorOutput: IHandle<ErrorEvent>
    {
        public void Handle(ErrorEvent args)
        {
            using (new ConsoleTextAttributesScope())
            {
                Console.ForegroundColor = ConsoleColor.Red;

                var message = args.Message ?? args.Exception?.Message;

                Console.Error.WriteLine(message);
            }
        }
    }
}
