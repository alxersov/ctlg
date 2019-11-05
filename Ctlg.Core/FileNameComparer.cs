using System;
using System.Collections.Generic;

namespace Ctlg.Core
{
    public class FileNameComparer : IComparer<File>
    {
        public int Compare(File x, File y)
        {
            if (x == null || x.Name == null)
            {
                if (y == null || y.Name == null)
                {
                    return 0;
                }

                return -1;
            }

            if (y == null || y.Name == null)
            {
                return 1;
            }

            return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
        }
    }
}
