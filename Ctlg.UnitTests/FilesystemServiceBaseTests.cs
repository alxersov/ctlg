using System;
// using System.IO;
using System.Linq;
using Autofac.Extras.Moq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Filesystem;
using Ctlg.Service;
using Ctlg.Service.Utils;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests
{
    public class FilesystemServiceBaseTests
    {
        [Test]
        public void EnumerateFiles_WithoutMask_EnumeratesAllFiles()
        {
            var service = CreateService("*");

            var files = service.EnumerateFiles("test_path").Select(f => f.Name);

            Assert.That(files, Is.EqualTo(new[] { "hello" }));
        }

        [Test]
        public void EnumerateFiles_WithMask_UsesMaskToEnumerateFiles()
        {
            var service = CreateService("some-mask");

            var files = service.EnumerateFiles("test_path", "some-mask").Select(f => f.Name);

            Assert.That(files, Is.EqualTo(new[] { "hello" }));
        }

        private FilesystemServiceBase CreateService(string enumerateFilesMask)
        {
            var directoryMock = new Mock<IFilesystemDirectory>();
            directoryMock.Setup(d => d.EnumerateFiles(It.Is<string>(mask => mask == enumerateFilesMask)))
                .Returns(new[] { new File("hello") });

            var mock = new Mock<FilesystemServiceBase>();
            mock.Setup(s => s.GetDirectory(It.Is<string>(path => path == "test_path")))
                .Returns(directoryMock.Object);
            return mock.Object;
        }
    }
}
