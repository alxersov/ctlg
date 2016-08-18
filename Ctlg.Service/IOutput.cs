namespace Ctlg.Filesystem.Service
{
    public interface IOutput
    {
        void Write(string message);
        void WriteLine(string message);
    }
}
