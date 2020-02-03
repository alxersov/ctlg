using System;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Commands;
using Ctlg.UnitTests.Fixtures;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests.Commands
{
    public class RebuildIndexCommandTests: CommandTestFixture<RebuildIndexCommand>
    {
        private string CurrentDir = "current-dir";
        private Mock<IFileStorage> FileStorageMock;

        [SetUp]
        public void Init()
        {
            FilesystemServiceMock.Setup(s => s.GetCurrentDirectory()).Returns(CurrentDir);
            FileStorageMock = FileStorageServiceMock.SetupGetFileStorage(CurrentDir, false, false);
        }

        [Test]
        public void RebuildsIndex()
        {
            Command.Execute();
            FileStorageMock.Verify(m => m.RebuildIndex(), Times.Once);
        }
    }
}
