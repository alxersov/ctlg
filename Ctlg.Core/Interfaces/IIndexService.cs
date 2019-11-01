using System;
using System.Collections.Generic;

namespace Ctlg.Core.Interfaces
{
    public interface IIndexService
    {
        void Add(byte[] hash);
        bool IsInIndex(byte[] hash);
        IEnumerable<byte[]> GetAllHashes();
    }
}
