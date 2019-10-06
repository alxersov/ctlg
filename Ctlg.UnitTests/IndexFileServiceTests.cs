using System;
using System.IO;
using Autofac.Extras.Moq;
using Ctlg.Core.Interfaces;
using Ctlg.Service;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests
{
    [TestFixture]
    public class IndexFileServiceTests
    {
        [Test]
        public void Save_WhenCalled_WritesIndexToFile()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IIndexService>()
                    .Setup(s => s.GetAllHashes())
                    .Returns(new[] { new byte[] { 1, 2 }, new byte[] { 3, 4 } });

                mock.Mock<ICtlgService>()
                    .SetupGet(s => s.IndexPath).Returns("index-file-path");

                var stream = new MemoryStream();
                mock.Mock<IFilesystemService>()
                    .Setup(s => s.CreateFileForWrite(It.Is<string>(path => path == "index-file-path")))
                    .Returns(stream);

                var service = mock.Create<IndexFileService>();
                service.Save();

                Assert.That(stream.ToArray(), Is.EqualTo(new byte[] { 1, 2, 3, 4 }));
            }
        }
    }
}
