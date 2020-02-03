using System;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Commands;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests.Fixtures
{
    public abstract class CommandTestFixture<S>: AutoMockTestFixture where S: ICommand
    {
        protected S Command { get; set; }

        protected Mock<ISnapshotService> SnapshotServiceMock;
        protected Mock<ICtlgService> CtlgServiceMock;
        protected Mock<IFileStorageService> FileStorageServiceMock;
        protected Mock<IFilesystemService> FilesystemServiceMock;


        [SetUp]
        public void CreateMocksAndCommand()
        {
            SnapshotServiceMock = AutoMock.Mock<ISnapshotService>();
            FileStorageServiceMock = AutoMock.Mock<IFileStorageService>();
            CtlgServiceMock = AutoMock.Mock<ICtlgService>();
            FilesystemServiceMock = AutoMock.Mock<IFilesystemService>();

            Command = AutoMock.Create<S>();
        }
    }
}
