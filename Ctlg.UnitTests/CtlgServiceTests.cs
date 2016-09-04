using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Autofac.Extras.Moq;
using Ctlg.Data.Model;
using Ctlg.Data.Service;
using Ctlg.Filesystem.Service;
using Ctlg.Service;
using Ctlg.Service.Commands;
using Moq;
using NUnit.Framework;
using File = Ctlg.Data.Model.File;

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
        public void AddDirectory_WhenDirectoryWithFiles_SavesAllFiles()
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
        public void AddDirectory_WhenDirectoryWithFiles_FilesHaveHashValues()
        {
            var fakeDir = CreateFakeDirWithOneFiles();

            var addedDirectory = AddDirectory(fakeDir);

            Assert.That(addedDirectory.Contents[0].Hashes.Count, Is.EqualTo(1));
            Assert.That(addedDirectory.Contents[0].Hashes[0], Is.EqualTo(new Hash(1, new byte[] {1, 2, 3, 4})));
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
        public void AddDirectory_WhenDirectoryWithFiles_OutputsFileHashes()
        {
            var fakeDir = CreateFakeDirWithTwoFiles();

            var output = AddDirectoryAndGetOutput(fakeDir);

            Assert.That(output, Does.Contain("01020304"));
        }


        [Test]
        public void AddDirectory_WhenDirectoryWithSubdirectories_SavesAllFiles()
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

        [Test]
        public void List_WhenOneEmptyDirectoryInDb_OutputsItsName()
        {
            var files = new List<File> { new File("test-dir", true) };

            var output = ListFilesAndGetOutput(files);

            Assert.That(output, Does.Contain("test-dir"));
        }

        [Test]
        public void List_WhenDirectoryWithSubdirInDb_OutputsAllNames()
        {
            var files = new List<File>
            {
                new File("test-dir", true)
                {
                    Contents = new List<File>
                    {
                        new File("test-subdir")
                        {
                            Contents = new List<File>
                            {
                                new File("test-file")
                            }
                        }
                    }
                }
            };

            var output = ListFilesAndGetOutput(files);

            Assert.That(output, Does.Contain("test-dir"));
            Assert.That(output, Does.Contain("test-subdir"));
            Assert.That(output, Does.Contain("test-file"));
        }

        [Test]
        public void List_FileHasHash_OutputsHash()
        {
            var files = new List<File>
            {
                new File("test-dir", true)
                {
                    Contents = new List<File>
                    {
                        new File("test-file")
                    }
                }
            };

            files[0].Contents[0].Hashes = new List<Hash> {new Hash(1, new byte[] {1, 0, 0xAB})};

            var output = ListFilesAndGetOutput(files);

            Assert.That(output, Does.Contain("0100AB").IgnoreCase);
        }

        [Test]
        public void Find_FileFound_OtputsFullPath()
        {
            var file = new File
            {
                Name = "1.txt",
                ParentFile = new File
                {
                    Name = "B",
                    ParentFile = new File
                    {
                        Name = "A"
                    }
                }
            };

            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IDataService>()
                    .Setup(d => d.GetFiles(It.IsAny<byte[]>()))
                    .Returns(new List<File> {file});

                var stringBuilder = new StringBuilder();

                mock.Mock<IOutput>()
                    .Setup(f => f.Write(It.IsAny<string>()))
                    .Callback<string>(message => stringBuilder.Append(message));
                mock.Mock<IOutput>()
                    .Setup(f => f.WriteLine(It.IsAny<string>()))
                    .Callback<string>(message => stringBuilder.AppendLine(message));

                var ctlg = mock.Create<CtlgService>();
                ctlg.FindFiles(new byte[0]);

                var output = stringBuilder.ToString();

                Assert.That(output, Does.Contain(@"A\B\1.txt"));
            }
        }

        [Test]
        public void ApplyDbMigrations_CallsDataService()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var ctlg = mock.Create<CtlgService>();

                ctlg.ApplyDbMigrations();

                mock.Mock<IDataService>().Verify(s => s.ApplyDbMigrations(), Times.Once);
            }
        }

        [Test]
        public void Execute_CallsCommandExecute()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var ctlg = mock.Create<CtlgService>();

                var command = new Mock<ICommand>();

                ctlg.Execute(command.Object);

                command.Verify(c => c.Execute(ctlg), Times.Once);
            }
        }

        private string ListFilesAndGetOutput(IList<File> files)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IDataService>()
                    .Setup(d => d.GetFiles())
                    .Returns(files);

                var stringBuilder = new StringBuilder();

                mock.Mock<IOutput>()
                    .Setup(f => f.Write(It.IsAny<string>()))
                    .Callback<string>(message => stringBuilder.Append(message));
                mock.Mock<IOutput>()
                    .Setup(f => f.WriteLine(It.IsAny<string>()))
                    .Callback<string>(message => stringBuilder.AppendLine(message));

                var ctlg = mock.Create<CtlgService>();
                ctlg.ListFiles();

                return stringBuilder.ToString();
            }
        }

        private static Mock<IFilesystemDirectory> CreateFakeEmptyDir()
        {
            var fakeDir = new Mock<IFilesystemDirectory>();
            fakeDir.Setup(d => d.Directory).Returns(new File("path", true) {FullPath = @"c:\some\full\path"});
            fakeDir.Setup(d => d.EnumerateDirectories()).Returns(new List<FilesystemDirectory>());
            fakeDir.Setup(d => d.EnumerateFiles()).Returns(new List<File>());
            return fakeDir;
        }

        private static Mock<IFilesystemDirectory> CreateFakeDirWithOneFiles()
        {
            var fakeDir = CreateFakeEmptyDir();
            fakeDir.Setup(d => d.EnumerateFiles()).Returns(new List<File>
            {
                new File("1.txt") {FullPath = @"c:\some\full\path\1.txt"},
            });
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
                    .Setup(f => f.GetDirectory(It.Is<string>(s => s == "somepath")))
                    .Returns(fakeDir.Object);
                mock.Mock<IHashService>()
                    .Setup(f => f.CalculateSha1(It.IsAny<Stream>()))
                    .Returns(new byte[] {1, 2, 3, 4});

                var ctlg = mock.Create<CtlgService>();
                ctlg.AddDirectory("somepath");

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
                    .Setup(f => f.GetDirectory(It.Is<string>(s => s == "somepath")))
                    .Returns(fakeDir.Object);
                mock.Mock<IHashService>()
                    .Setup(f => f.CalculateSha1(It.IsAny<Stream>()))
                    .Returns(new byte[] { 1, 2, 3, 4 });

                var ctlg = mock.Create<CtlgService>();
                ctlg.AddDirectory("somepath");

                return stringBuilder.ToString();
            }
        }
    }
}
