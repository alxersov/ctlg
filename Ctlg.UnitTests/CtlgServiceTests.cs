﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac.Extras.Moq;
using Autofac.Features.Indexed;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Data;
using Ctlg.Filesystem;
using Ctlg.Service;
using Ctlg.Service.Commands;
using Ctlg.Service.Events;
using Moq;
using NUnit.Framework;
using File = Ctlg.Core.File;

namespace Ctlg.UnitTests
{
    public class CtlgServiceTests: BaseTestFixture
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
            var fakeDir = CreateFakeDirWithOneFile();

            var addedDirectory = AddDirectory(fakeDir);

            Assert.That(addedDirectory.Contents[0].Hashes.Count, Is.EqualTo(1));
            Assert.That(addedDirectory.Contents[0].Hashes[0], Is.EqualTo(new Hash(1, new byte[] {1, 2, 3, 4})));
        }


        [Test]
        public void AddDirectory_WhenDirectoryWithFiles_Raises2FileFoundEvents()
        {
            var events = SetupEvents<FileFound>();

            var fakeDir = CreateFakeDirWithTwoFiles();
            
            AddDirectory(fakeDir);

            Assert.That(events.Count, Is.EqualTo(2));
            Assert.That(events[0].FullPath, Does.Contain("1.txt"));
            Assert.That(events[1].FullPath, Does.Contain("foo.bar"));
        }

        [Test]
        public void AddDirectory_WhenDirectoryWithFiles_RaisesHashCalculatedEvent()
        {
            var events = SetupEvents<HashCalculated>();

            var fakeDir = CreateFakeDirWithOneFile();

            AddDirectory(fakeDir);

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].Hash, Is.EquivalentTo(new byte[] {1,2,3,4}));
        }


        [Test]
        public void AddDirectory_WhenDirectoryWithSubdirectories_SavesAllFiles()
        {
            var fakeSubdir = CreateFakeEmptyDir();
            fakeSubdir.Setup(d => d.EnumerateFiles(It.IsAny<string>())).Returns(new List<File>
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
        public void AddDirectory_WhenEmptyDirectory_RaisesDirectoryFoundEvent()
        {
            var events = SetupEvents<DirectoryFound>();

            var fakeDir = CreateFakeEmptyDir();
            AddDirectory(fakeDir);

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].FullPath, Is.EqualTo(@"c:\some\full\path"));
        }

        [Test]
        public void List_WhenDirectoryWithSubdirInDb_RaisesTreeItemEnumerated()
        {
            var events = SetupEvents<TreeItemEnumerated>();

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

            ListFiles(files);

            Assert.That(events.Count, Is.EqualTo(3));
            Assert.That(events[0].File.Name, Is.EqualTo("test-dir"));
            Assert.That(events[1].File.Name, Is.EqualTo("test-subdir"));
            Assert.That(events[2].File.Name, Is.EqualTo("test-file"));
        }

        [Test]
        public void Find_FileFound_RaisesFileFoundInDbEvent()
        {
            var events = SetupEvents<FileFoundInDb>();

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
                    .Setup(d => d.GetFiles(It.IsAny<Hash>(), It.IsAny<long?>(), It.IsAny<string>()))
                    .Returns(new List<File> {file});

                var ctlg = mock.Create<CtlgService>();
                ctlg.FindFiles(new Hash(1, new byte[0]), null, "*");

                Assert.That(events.Count, Is.EqualTo(1));
                Assert.That(events[0].File.BuildFullPath(), Does.Contain(@"A\B\1.txt"));
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

        private void ListFiles(IList<File> files)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IDataService>()
                    .Setup(d => d.GetFiles())
                    .Returns(files);

                var ctlg = mock.Create<CtlgService>();
                ctlg.ListFiles();
            }
        }

        private static Mock<IFilesystemDirectory> CreateFakeEmptyDir()
        {
            var fakeDir = new Mock<IFilesystemDirectory>();
            fakeDir.Setup(d => d.Directory).Returns(new File("path", true) {FullPath = @"c:\some\full\path"});
            fakeDir.Setup(d => d.EnumerateDirectories()).Returns(new List<FilesystemDirectory>());
            fakeDir.Setup(d => d.EnumerateFiles(It.IsAny<string>())).Returns(new List<File>());
            return fakeDir;
        }

        private static Mock<IFilesystemDirectory> CreateFakeDirWithOneFile()
        {
            var fakeDir = CreateFakeEmptyDir();
            fakeDir.Setup(d => d.EnumerateFiles(It.IsAny<string>())).Returns(new List<File>
            {
                new File("1.txt") {FullPath = @"c:\some\full\path\1.txt"},
            });
            return fakeDir;
        }


        private static Mock<IFilesystemDirectory> CreateFakeDirWithTwoFiles()
        {
            var fakeDir = CreateFakeEmptyDir();
            fakeDir.Setup(d => d.EnumerateFiles(It.IsAny<string>())).Returns(new List<File>
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

                mock.Mock<IDataService>()
                    .Setup(d => d.GetHashAlgorithm(It.IsAny<string>())).Returns(new HashAlgorithm()
                    {
                        HashAlgorithmId = 1,
                        Name = "XHASH"
                    });

                mock.Mock<IFilesystemService>()
                    .Setup(f => f.GetDirectory(It.Is<string>(s => s == "somepath")))
                    .Returns(fakeDir.Object);

                var hashFunctionMock = new Mock<IHashFunction>();
                hashFunctionMock.Setup(f => f.CalculateHash(It.IsAny<Stream>()))
                                .Returns(new Hash(1, new byte[] {1, 2, 3, 4}));

                var index = new Index<string, IHashFunction>();
                index.Add("XHASH", hashFunctionMock.Object);
                mock.Provide<IIndex<string, IHashFunction>>(index);


                var addCommand = mock.Create<AddCommand>();

                addCommand.Path = "somepath";
                addCommand.HashFunctionName = "XHASH";

                var ctlg = mock.Create<CtlgService>();
                addCommand.Execute(ctlg);

                return addedDirectory;
            }
        }
    }
}
