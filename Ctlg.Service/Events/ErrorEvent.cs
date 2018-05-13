using System;

namespace Ctlg.Service.Events
{
    public class ErrorEvent: IDomainEvent
    {
        public ErrorEvent(Exception exception)
        {
            Exception = exception;
        }

        public ErrorEvent(string message, Exception exception = null)
        {
            Message = message;
            Exception = exception;
        }

        public Exception Exception { get; }
        public string Message { get; }
    }
}
