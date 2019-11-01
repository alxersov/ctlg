using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.Moq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service;
using Ctlg.Service.Commands;
using Ctlg.Service.Events;
using Ctlg.Service.Utils;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests
{
    public class RebuildIndexCommandTests: AutoMockTestFixture
    {
        private Mock<IIndexService> IndexServiceMock;
        private Mock<IIndexFileService> IndexFileServiceMock;
        private string Dir1;
        private string Dir2;
        private string File1;
        private string File2;
        private string File3;
        private IList<Warning> Warnings;

        [SetUp]
        public void Setup()
        {
            IndexServiceMock = AutoMock.Mock<IIndexService>();
            IndexFileServiceMock = AutoMock.Mock<IIndexFileService>();

            Dir1 = "aa";
            Dir2 = "bb";
            File1 = "aa0002030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f";
            File2 = "aa0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f";
            File3 = "bb0202030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f";

            Warnings = SetupEvents<Warning>();
        }

        [Test]
        public void Execute_AddsHashesToIndex()
        {
            SetupStorageDir();

            ExecuteCommand();

            VerifyIndexSerivceAdd(File1, File2, File3).VerifyNoOtherCalls();
            IndexFileServiceMock.Verify(s => s.Save(), Times.Once);
        }

        [Test]
        public void Execute_SkipsFilesWithIncorrectNames()
        {
            File2 = "foo";
            SetupStorageDir();

            ExecuteCommand();

            VerifyIndexSerivceAdd(File1, File3).VerifyNoOtherCalls();
            Assert.That(Warnings.Any(w => w.Message.Contains("Unexpected file in storage: foo")));
        }

        [Test]
        public void Execute_SkipsDirectoriesWithIncorrectNames()
        {
            Dir2 = "foo";
            SetupStorageDir();

            ExecuteCommand();

            VerifyIndexSerivceAdd(File1, File2).VerifyNoOtherCalls();
            Assert.That(Warnings.Any(w => w.Message.Contains("Unexpected directory in storage: foo")));
        }

        private void SetupStorageDir()
        {
            AutoMock.Mock<ICtlgService>().SetupGet(s => s.FileStorageDirectory).Returns("files-dir");

            var dir1 = MockHelper.MockDirectory(Dir1, new File[] { new File(File1), new File(File2) });
            var dir2 = MockHelper.MockDirectory(Dir2, new File[] { new File(File3) });
            var filesDir = MockHelper.MockDirectory("files-dir", null, new IFilesystemDirectory[] { dir1, dir2 });

            AutoMock.Mock<IFilesystemService>().Setup(s => s.GetDirectory(It.Is<string>(p => p == "files-dir"))).Returns(filesDir);
        }

        private Mock<IIndexService> VerifyIndexSerivceAdd(params string[] hashes)
        {
            foreach (var hash in hashes)
            {
                var bytes = FormatBytes.ToByteArray(hash);
                IndexServiceMock.Verify(s => s.Add(bytes));
            }

            return IndexServiceMock;
        }

        private void ExecuteCommand()
        {
            var command = AutoMock.Create<RebuildIndexCommand>();
            command.Execute();
        }
    }
}
