using System;
using Autofac.Core;
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

                var message = args.Message ?? GetMessage(args.Exception);

                Console.Error.WriteLine(message);
            }
        }

        private string GetMessage(Exception ex)
        {
            var dependencyResolutionException = ex as DependencyResolutionException;
            if (dependencyResolutionException != null)
            {
                return GetMessage(dependencyResolutionException.InnerException);
            }

            return ex?.Message;
        }
    }
}
