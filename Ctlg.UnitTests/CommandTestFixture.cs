using System;
using Ctlg.Core.Interfaces;
using Ctlg.Service;
using Ctlg.Service.Commands;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests
{
    public abstract class CommandTestFixture<S>: AutoMockTestFixture where S: ICommand
    {
        protected S Command { get; set; }

        protected Mock<ISnapshotService> SnapshotServiceMock;
        protected Mock<ICtlgService> CtlgServiceMock;
        protected Mock<IFileStorageService> FileStorageServiceMock;
        protected Mock<IIndexFileService> IndexFileServiceMock;


        [SetUp]
        public void CreateMocksAndCommand()
        {
            SnapshotServiceMock = AutoMock.Mock<ISnapshotService>();
            FileStorageServiceMock = AutoMock.Mock<IFileStorageService>();
            CtlgServiceMock = AutoMock.Mock<ICtlgService>();
            IndexFileServiceMock = AutoMock.Mock<IIndexFileService>();

            Command = AutoMock.Create<S>();
        }
    }
}
