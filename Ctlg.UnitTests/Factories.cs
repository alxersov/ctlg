using System;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Moq;

namespace Ctlg.UnitTests
{
    public static class Factories
    {
        public static SnapshotRecord[] SnapshotRecords
        {
            get
            {
                return new[]
                {
                    new SnapshotRecord($"{Hash1} 2018-04-22T18:05:12.0000000Z 11 1.txt", HashAlgorithm),
                    new SnapshotRecord($"{Hash2} 2019-01-22T00:00:00.0000000Z 12345 foo/bar.txt", HashAlgorithm)
                };
            }
        }

        public static Mock<ISnapshot> CreateSnapshotMock(string name, string timestamp)
        {
            var snapshotMock = new Mock<ISnapshot>();
            snapshotMock.SetupGet(m => m.Name).Returns(name);
            snapshotMock.SetupGet(m => m.Timestamp).Returns(timestamp);
            snapshotMock.Setup(m => m.EnumerateFiles()).Returns(new[] { SnapshotRecords[0] });
            return snapshotMock;
        }

        public static HashAlgorithm HashAlgorithm
        {
            get
            {
                return new HashAlgorithm() { HashAlgorithmId = 2, Name = "SHA-256", Length = 32 };
            }
        }

        public static Config Config
        {
            get
            {
                return new Config
                {
                    Path = "home",
                    HashAlgorithmName = "SHA-256"
                };
            }
        }

        private const string Hash1 = "64ec88ca00b268e5ba1a35678a1b5316d212f4f366b2477232534a8aeca37f3c";
        private const string Hash2 = "0123456789012345678901234567890123456789012345678901234567890123";
    }
}
