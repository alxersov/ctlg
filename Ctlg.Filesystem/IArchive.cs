using System;
using System.Collections.Generic;
using Ctlg.Core;

namespace Ctlg.Filesystem
{
    public interface IArchive : IDisposable
    {
        IEnumerable<File> EnumerateEntries();
    }
}
