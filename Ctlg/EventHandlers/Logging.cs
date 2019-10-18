using System;
using Ctlg.Service;
using Ctlg.Service.Events;

namespace Ctlg.EventHandlers
{
    public class Logging : IHandle<ErrorEvent>, IHandle<Warning>
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public void Handle(ErrorEvent args)
        {
            Logger.Error(args.Exception, args.Message);
        }

        public void Handle(Warning args)
        {
            Logger.Warn(args.Message);
        }
    }
}
