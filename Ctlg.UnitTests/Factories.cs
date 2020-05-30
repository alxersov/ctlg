using System;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Utils;
using Moq;

namespace Ctlg.UnitTests
{
    public static class Factories
    {
        public static File[] SnapshotRecords
        {
            get
            {
                var file1 = new File("1.txt")
                {
                    Size = 11,
                    FileModifiedDateTime = new DateTime(2018, 04, 22, 18, 05, 12, DateTimeKind.Utc)
                };
                file1.Hashes.Add(new Hash(HashAlgorithm, FormatBytes.ToByteArray(Hash1)));

                var file2 = new File("foo/bar.txt")
                {
                    Size = 12345,
                    FileModifiedDateTime = new DateTime(2019, 01, 22, 0, 0, 0, DateTimeKind.Utc)
                };
                file2.Hashes.Add(new Hash(HashAlgorithm, FormatBytes.ToByteArray(Hash2)));

                return new[] { file1, file2 };
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
                    HashAlgorithmName = "SHA-256",
                    SnapshotServiceName = "TXT"
                };
            }
        }

        private const string Hash1 = "64ec88ca00b268e5ba1a35678a1b5316d212f4f366b2477232534a8aeca37f3c";
        private const string Hash2 = "0123456789012345678901234567890123456789012345678901234567890123";
    }
}
