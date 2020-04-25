using System;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Commands;
using Ctlg.UnitTests.Fixtures;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests.Commands
{
    public class RestoreCommandTests: CommandTestFixture<RestoreCommand>
    {
        public string Name = "test-name";
        public string Date = "2019-01-01";
        public string Path = "foo";
        public string DestinationFilePath = "foo/something";
        private readonly string CurrentDirectory = "home";
        private ISnapshot Snapshot;
        private SnapshotRecord SnapshotRecord;
        private Mock<IFileStorage> FileStorageMock;


        [SetUp]
        public void Init()
        {
            SnapshotServiceMock.Setup(s => s.GetSnapshot(CurrentDirectory, "SHA-256", Name, Date))
                .Returns(() => Snapshot);

            Snapshot = Factories.CreateSnapshotMock(Name, Date).Object;
            SnapshotRecord = Factories.SnapshotRecords[0];

            FileStorageMock = FileStorageServiceMock.SetupGetFileStorage(CurrentDirectory);

            FilesystemServiceMock.Setup(s => s.CombinePath(Path, SnapshotRecord.Name))
                .Returns(DestinationFilePath);
        }

        [Test]
        public void Execute_WhenSnapshotNotFound_ThrowsException()
        {
            Snapshot = null;
            Assert.That(() => Execute(),
                Throws.TypeOf<Exception>()
                    .With.Message.Contain($"Snapshot {Name} is not found"));
        }

        [Test]
        public void Execute_CopiesFileToCorrectDestination()
        {
            Execute();

            FileStorageMock.Verify(m => m.CopyFileTo(SnapshotRecord.Hash.ToString(), DestinationFilePath));
        }

        private void Execute()
        {
            Command.Path = Path;
            Command.Name = Name;
            Command.Date = Date;

            Command.Execute(Factories.Config);
        }
    }
}
