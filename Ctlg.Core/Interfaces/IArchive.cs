using System;
using System.Collections.Generic;

namespace Ctlg.Core.Interfaces
{
    public interface IArchive : IDisposable
    {
        IEnumerable<File> EnumerateEntries();
    }
}
