using System;

namespace Ctlg.Service.Events
{
    public class ExceptionEvent: IDomainEvent
    {
        public ExceptionEvent(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}
