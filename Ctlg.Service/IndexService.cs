using System;
using System.Collections.Generic;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service
{
    public class IndexService: IIndexService
    {
        public void Add(byte[] hash)
        {
            _set.Add(hash);
        }

        public IEnumerable<byte[]> GetAllHashes()
        {
            return _set;
        }

        private SortedSet<byte[]> _set = new SortedSet<byte[]>(new ByteArrayComparer());
    }
}
