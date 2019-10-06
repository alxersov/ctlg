using System;
using System.Linq;
using Autofac.Extras.Moq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service;
using Ctlg.Service.Commands;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests
{
    [TestFixture]
    public class RebuildIndexCommandTests
    {
        [Test]
        public void Execute_AddsHashesToIndex()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<ICtlgService>()
                    .SetupGet(s => s.FileStorageDirectory).Returns("files-dir");

                var dir1 = MockHelper.MockDirectory("aa", new File[] { new File("aa01"), new File("aa02") });
                var dir2 = MockHelper.MockDirectory("bb", new File[] { new File("bb01") });
                var filesDir = MockHelper.MockDirectory("files-dir", null, new IFilesystemDirectory[] { dir1, dir2 });

                mock.Mock<IFilesystemService>().Setup(s => s.GetDirectory(It.Is<string>(p => p == "files-dir"))).Returns(filesDir);

                var indexService = mock.Mock<IIndexService>();
                var indexFileService = mock.Mock<IIndexFileService>();

                indexService.Setup(s => s.Add(new byte[] { 0xaa, 0x1 }));
                indexService.Setup(s => s.Add(new byte[] { 0xaa, 0x2 }));
                indexService.Setup(s => s.Add(new byte[] { 0xbb, 0x1 }));

                var command = mock.Create<RebuildIndexCommand>();
                command.Execute();

                indexService.VerifyAll();
                indexFileService.Verify(s => s.Save(), Times.Once);
            }
        }
    }
}
