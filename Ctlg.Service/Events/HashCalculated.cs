namespace Ctlg.Service.Events
{
    public class HashCalculated: IDomainEvent
    {
        public HashCalculated(string path, byte[] hash)
        {
            Path = path;
            Hash = hash;
        }

        public string Path { get; }
        public byte[] Hash { get; }
    }
}
