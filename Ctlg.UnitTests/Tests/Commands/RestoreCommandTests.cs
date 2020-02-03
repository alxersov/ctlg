using System;
using Autofac.Extras.Moq;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Commands;
using Ctlg.Service.Events;
using Ctlg.UnitTests.Fixtures;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests.Commands
{
    public class RestoreCommandTests: CommandTestFixture<RestoreCommand>
    {
        public string Name = "test-name";
        public string Date = "2019-01-01";
        public string Path = "foo";
        private readonly string CurrentDirectory = "current-dir";
        public ISnapshot Snapshot;


        [SetUp]
        public void Init()
        {
            FilesystemServiceMock.Setup(s => s.GetCurrentDirectory()).Returns(CurrentDirectory);
            SnapshotServiceMock.Setup(s => s.GetSnapshot(CurrentDirectory, Name, Date))
                .Returns(() => Snapshot);

            Snapshot = Factories.CreateSnapshotMock(Name, Date).Object;

            var fileStorageMock = FileStorageServiceMock.SetupGetFileStorage(CurrentDirectory, true, true);
        }

        [Test]
        public void Execute_WhenSnapshotNotFound_ThrowsException()
        {
            Snapshot = null;
            Assert.That(() => Execute(),
                Throws.TypeOf<Exception>()
                    .With.Message.Contain($"Snapshot {Name} is not found"));
        }

        private void Execute()
        {
            Command.Path = Path;
            Command.Name = Name;
            Command.Date = Date;

            Command.Execute();
        }
    }
}
