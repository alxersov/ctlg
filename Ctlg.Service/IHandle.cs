namespace Ctlg.Service
{
    public interface IHandle<T> where T : IDomainEvent
    {
        void Handle(T args);
    }
}