using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.Moq;
using Ctlg.Data.Model;
using Ctlg.Data.Service;
using Ctlg.Filesystem.Service;
using Ctlg.Service;
using Ctlg.Service.Commands;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests
{
    [TestFixture]
    public class AddCommandTests
    {
        [Test]
        public void Execute_WhenEmptyDirectory_SavesItWihtFullPathAsName()
        {
            var fakeDir = CreateFakeEmptyDir();

            var addedDirectory = AddedDirectory(fakeDir);

            Assert.That(addedDirectory, Is.Not.Null);
            Assert.That(addedDirectory.Name, Is.EqualTo(@"c:\some\full\path"));
            Assert.That(addedDirectory.IsDirectory, Is.True);
        }

        [Test]
        public void Execute_WhenDirectoryWithFiles_SavesAll()
        {
            var fakeDir = CreateFakeEmptyDir();
            fakeDir.Setup(d => d.EnumerateFiles()).Returns(new List<FilesystemEntry>
            {
                new FilesystemEntry {File = new File("1.txt"), FullPath = @"c:\some\full\path\1.txt"},
                new FilesystemEntry {File = new File("foo.bar"), FullPath = @"c:\some\full\path\foo.bar"},
            });

            var addedDirectory = AddedDirectory(fakeDir);

            Assert.That(addedDirectory, Is.Not.Null);
            Assert.That(addedDirectory.Contents.Count, Is.EqualTo(2));
            Assert.That(addedDirectory.Contents.Count(f => !f.IsDirectory), Is.EqualTo(2));
            Assert.That(addedDirectory.Contents.Count(f => f.Name == "1.txt"), Is.EqualTo(1));
            Assert.That(addedDirectory.Contents.Count(f => f.Name == "foo.bar"), Is.EqualTo(1));
        }

        [Test]
        public void Execute_WhenDirectoryWithSubdirectories_SavesAll()
        {
            var fakeSubdir = CreateFakeEmptyDir();
            fakeSubdir.Setup(d => d.EnumerateFiles()).Returns(new List<FilesystemEntry>
            {
                new FilesystemEntry {File = new File("1.txt"), FullPath = @"c:\some\full\path\1.txt"}
            });

            var fakeDir = CreateFakeEmptyDir();
            fakeDir.Setup(d => d.EnumerateDirectories()).Returns(new List<IFilesystemDirectory> { fakeSubdir.Object });

            var addedDirectory = AddedDirectory(fakeDir);

            Assert.That(addedDirectory, Is.Not.Null);
            Assert.That(addedDirectory.Contents.Count, Is.EqualTo(1));

            var subdir = addedDirectory.Contents[0];

            Assert.That(subdir.Name, Is.EqualTo("path"));
            Assert.That(subdir.IsDirectory, Is.True);
            Assert.That(subdir.Contents.Count, Is.EqualTo(1));

            var file = subdir.Contents[0];
            Assert.That(file.Name, Is.EqualTo("1.txt"));
            Assert.That(file.IsDirectory, Is.False);
        }

        private static Mock<IFilesystemDirectory> CreateFakeEmptyDir()
        {
            var fakeDir = new Mock<IFilesystemDirectory>();
            fakeDir.Setup(d => d.FullPath).Returns(@"c:\some\full\path");
            fakeDir.Setup(d => d.File).Returns(new File("path", true));
            fakeDir.Setup(d => d.EnumerateDirectories()).Returns(new List<FilesystemDirectory>());
            fakeDir.Setup(d => d.EnumerateFiles()).Returns(new List<FilesystemEntry>());
            return fakeDir;
        }

        private static File AddedDirectory(Mock<IFilesystemDirectory> fakeDir)
        {
            using (var mock = AutoMock.GetLoose())
            {
                File addedDirectory = null;
                mock.Mock<IDataService>()
                    .Setup(d => d.AddDirectory(It.IsAny<File>()))
                    .Callback<File>(f => addedDirectory = f);

                mock.Mock<IFilesystemService>()
                    .Setup(f => f.GetDirectory(It.IsAny<string>()))
                    .Returns(fakeDir.Object);

                var ctlg = mock.Create<CtlgService>();
                var addCommand = new AddCommand
                {
                    Path = "anything"
                };

                ctlg.Execute(addCommand);

                return addedDirectory;
            }
        }
    }
}
