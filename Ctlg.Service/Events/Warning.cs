using System;
namespace Ctlg.Service.Events
{
    public class Warning: IDomainEvent
    {
        public Warning(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}
