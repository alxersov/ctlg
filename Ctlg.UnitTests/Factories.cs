using System;
using Ctlg.Core;

namespace Ctlg.UnitTests
{
    public static class Factories
    {
        public static SnapshotFile CreateSnapshotFile()
        {
            var name = "foo";
            var date = "2019-01-01_00-00-00";
            var fullPath = $"some/path/{name}/{date}";

            return new SnapshotFile(name, date, fullPath);
        }

        public static SnapshotRecord[] SnapshotRecords
        {
            get
            {
                return new[]
                {
                    new SnapshotRecord($"{Hash1} 2018-04-22T18:05:12.0000000Z 11 1.txt"),
                    new SnapshotRecord($"{Hash2} 2019-01-22T00:00:00.0000000Z 12345 foo/bar.txt")
                };
            }
        }

        private const string Hash1 = "64ec88ca00b268e5ba1a35678a1b5316d212f4f366b2477232534a8aeca37f3c";
        private const string Hash2 = "0123456789012345678901234567890123456789012345678901234567890123";
    }
}
