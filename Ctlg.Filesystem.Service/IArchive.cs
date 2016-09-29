using System;
using System.Collections.Generic;
using Ctlg.Data.Model;

namespace Ctlg.Filesystem.Service
{
    public interface IArchive : IDisposable
    {
        IEnumerable<File> EnumerateEntries();
    }
}
