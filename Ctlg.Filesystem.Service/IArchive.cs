using System;
using System.Collections.Generic;
using Ctlg.Core;

namespace Ctlg.Filesystem.Service
{
    public interface IArchive : IDisposable
    {
        IEnumerable<File> EnumerateEntries();
    }
}
