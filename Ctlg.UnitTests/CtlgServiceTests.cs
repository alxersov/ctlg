using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Extras.Moq;
using Ctlg.Data.Model;
using Ctlg.Data.Service;
using Ctlg.Filesystem.Service;
using Ctlg.Service;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests
{
    [TestFixture]
    public class CtlgServiceTests
    {
        [Test]
        public void AddDirectory_WhenEmptyDirectory_SavesItWihtFullPathAsName()
        {
            var fakeDir = CreateFakeEmptyDir();

            var addedDirectory = AddDirectory(fakeDir);

            Assert.That(addedDirectory, Is.Not.Null);
            Assert.That(addedDirectory.Name, Is.EqualTo(@"c:\some\full\path"));
            Assert.That(addedDirectory.IsDirectory, Is.True);
        }

        [Test]
        public void AddDirectory_WhenDirectoryWithFiles_SavesAll()
        {
            var fakeDir = CreateFakeDirWithTwoFiles();

            var addedDirectory = AddDirectory(fakeDir);

            Assert.That(addedDirectory, Is.Not.Null);
            Assert.That(addedDirectory.Contents.Count, Is.EqualTo(2));
            Assert.That(addedDirectory.Contents.Count(f => !f.IsDirectory), Is.EqualTo(2));
            Assert.That(addedDirectory.Contents.Count(f => f.Name == "1.txt"), Is.EqualTo(1));
            Assert.That(addedDirectory.Contents.Count(f => f.Name == "foo.bar"), Is.EqualTo(1));
        }

        [Test]
        public void AddDirectory_WhenDirectoryWithFiles_OutputsTheirNames()
        {
            var fakeDir = CreateFakeDirWithTwoFiles();

            var output = AddDirectoryAndGetOutput(fakeDir);

            Assert.That(output, Does.Contain("1.txt"));
            Assert.That(output, Does.Contain("foo.bar"));
        }


        [Test]
        public void AddDirectory_WhenDirectoryWithSubdirectories_SavesAll()
        {
            var fakeSubdir = CreateFakeEmptyDir();
            fakeSubdir.Setup(d => d.EnumerateFiles()).Returns(new List<File>
            {
                new File("1.txt") {FullPath = @"c:\some\full\path\1.txt"}
            });

            var fakeDir = CreateFakeEmptyDir();
            fakeDir.Setup(d => d.EnumerateDirectories()).Returns(new List<IFilesystemDirectory> { fakeSubdir.Object });

            var addedDirectory = AddDirectory(fakeDir);

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

        [Test]
        public void AddDirectory_WhenEmptyDirectory_OutputFullPath()
        {
            var fakeDir = CreateFakeEmptyDir();

            var output = AddDirectoryAndGetOutput(fakeDir);

            Assert.That(output, Is.Not.Empty);
            Assert.That(output, Does.Contain(@"c:\some\full\path"));
        }

        private static Mock<IFilesystemDirectory> CreateFakeEmptyDir()
        {
            var fakeDir = new Mock<IFilesystemDirectory>();
            fakeDir.Setup(d => d.Directory).Returns(new File("path", true) {FullPath = @"c:\some\full\path"});
            fakeDir.Setup(d => d.EnumerateDirectories()).Returns(new List<FilesystemDirectory>());
            fakeDir.Setup(d => d.EnumerateFiles()).Returns(new List<File>());
            return fakeDir;
        }

        private static Mock<IFilesystemDirectory> CreateFakeDirWithTwoFiles()
        {
            var fakeDir = CreateFakeEmptyDir();
            fakeDir.Setup(d => d.EnumerateFiles()).Returns(new List<File>
            {
                new File("1.txt") {FullPath = @"c:\some\full\path\1.txt"},
                new File("foo.bar") {FullPath = @"c:\some\full\path\foo.bar"}
            });
            return fakeDir;
        }

        private static File AddDirectory(Mock<IFilesystemDirectory> fakeDir)
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
                ctlg.AddDirectory("");

                return addedDirectory;
            }
        }

        private static string AddDirectoryAndGetOutput(Mock<IFilesystemDirectory> fakeDir)
        {
            using (var mock = AutoMock.GetLoose())
            {
                var stringBuilder = new StringBuilder();

                mock.Mock<IOutput>()
                    .Setup(f => f.Write(It.IsAny<string>()))
                    .Callback<string>(message => stringBuilder.Append(message));
                mock.Mock<IOutput>()
                    .Setup(f => f.WriteLine(It.IsAny<string>()))
                    .Callback<string>(message => stringBuilder.AppendLine(message));

                mock.Mock<IFilesystemService>()
                    .Setup(f => f.GetDirectory(It.IsAny<string>()))
                    .Returns(fakeDir.Object);

                var ctlg = mock.Create<CtlgService>();
                ctlg.AddDirectory("");

                return stringBuilder.ToString();
            }
        }
    }
}
