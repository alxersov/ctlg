using System;
using Moq;
using NUnit.Framework;
using Autofac.Extras.Moq;
using Ctlg.Core.Interfaces;
using Ctlg.Core;
using Ctlg.Service.Commands;
using System.Collections.Generic;
using System.Linq;

namespace Ctlg.UnitTests
{
    public class FileEnumerateTests: BaseTestFixture
    {
        [TestCase(null, "*")]
        [TestCase("foo", "foo")]
        public void ReadTree_CreatesTree(string searchPatternParam, string searchPatternActuallyUsed)
        {
            using (var mock = AutoMock.GetLoose())
            {
                var file1 = new File("file1");
                var file2 = new File("file2");
                var nestedDir = new File("nested", true);
                var rootDir = new File("root", true);

                var nestedDirMock = new Mock<IFilesystemDirectory>();
                nestedDirMock.SetupGet(d => d.Directory).Returns(nestedDir);
                nestedDirMock.Setup(d => d.EnumerateFiles(It.Is<string>(searchPattern => searchPattern == searchPatternActuallyUsed)))
                    .Returns(new[] { file1, file2 });
                var rootDirMock = new Mock<IFilesystemDirectory>();
                rootDirMock.SetupGet(d => d.Directory).Returns(rootDir);
                rootDirMock.Setup(d => d.EnumerateDirectories()).Returns(new[] { nestedDirMock.Object });


                mock.Mock<IFilesystemService>()
                    .Setup(s => s.GetDirectory(It.Is<string>(path => path == "my-path")))
                    .Returns(rootDirMock.Object);

                var treeProvider = mock.Create<FileEnumerateStep>();

                var tree = treeProvider.ReadTree("my-path", searchPatternParam);

                rootDirMock.VerifyAll();
                nestedDirMock.VerifyAll();

                Assert.That(tree, Is.EqualTo(rootDir));
                Assert.That(tree.Contents, Is.EqualTo(new[] { nestedDir }));
                Assert.That(tree.Contents[0].Contents, Is.EqualTo(new[] { file1, file2 }));
            }
        }
    }
}
