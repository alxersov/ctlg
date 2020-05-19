using System;
using System.Collections.Generic;

namespace Ctlg.Core.Interfaces
{
    public interface ISnapshot
    {
        string Name { get; }
        string Timestamp { get; }
        IEnumerable<File> EnumerateFiles();
        ISnapshotWriter GetWriter();
    }
}
