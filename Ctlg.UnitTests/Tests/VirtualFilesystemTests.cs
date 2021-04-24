using System;
using System.Linq;
using Ctlg.UnitTests.TestDoubles;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests
{
    [TestFixture]
    public class VirtualFilesystemTests
    {
        public VirtualFilesystemTests()
        {
        }

        [Test]
        public void Stores_file_content()
        {
            var fs = new VirtualFileSystem();

            using (var stream = fs.CreateFileForWrite("foo"))
            {
                stream.WriteByte(123);
            }

            using (var stream = fs.OpenFileForRead("foo"))
            {
                Assert.That(stream.ReadByte(), Is.EqualTo(123));
                Assert.That(stream.ReadByte(), Is.EqualTo(-1));
            }
        }

        [Test]
        public void Stores_file_size()
        {
            var fs = new VirtualFileSystem();

            using (var stream = fs.CreateFileForWrite("foo"))
            {
                stream.Write(new byte[] { 1, 2, 3, 4, 5}, 0, 5);
            }

            Assert.That(fs.GetFileSize("foo"), Is.EqualTo(5));
        }

        [Test]
        public void When_file_does_not_exist()
        {
            var fs = new VirtualFileSystem();

            Assert.That(fs.FileExists("foo"), Is.False);
        }

        [Test]
        public void When_file_exists()
        {
            var fs = new VirtualFileSystem();
            fs.CreateFileForWrite("foo");

            Assert.That(fs.FileExists("foo"), Is.True);
        }

        [Test]
        public void Combines_path()
        {
            var fs = new VirtualFileSystem();

            Assert.That(fs.CombinePath("foo", "bar"), Is.EqualTo("foo/bar"));
        }

        [Test]
        public void GetDirectoryName_when_file_is_in_root()
        {
            var fs = new VirtualFileSystem();

            Assert.That(fs.GetDirectoryName("foo"), Is.EqualTo(""));
        }

        [Test]
        public void GetDirectoryName()
        {
            var fs = new VirtualFileSystem();

            Assert.That(fs.GetDirectoryName("foo/bar/1/2/3"), Is.EqualTo("foo/bar/1/2"));
        }

        [Test]
        public void When_directory_does_not_exist()
        {
            var fs = new VirtualFileSystem();

            fs.CreateDirectory("foo/1");

            Assert.That(fs.DirectoryExists("foo/1/2"), Is.False);
        }

        [Test]
        public void When_directory_exists()
        {
            var fs = new VirtualFileSystem();
            fs.CreateDirectory("foo/1/2");

            Assert.That(fs.DirectoryExists("foo/1/2"), Is.True);
        }

        [Test]
        public void SetFile_sets_file_content()
        {
            var fs = new VirtualFileSystem();
            fs.SetFile("foo", "test");

            Assert.That(fs.GetFileAsString("foo"), Is.EqualTo("test"));
        }

        [Test]
        public void Move()
        {
            var fs = new VirtualFileSystem();
            fs.SetFile("foo", "test");

            fs.Move("foo", "baz");

            Assert.That(fs.GetFileAsString("baz"), Is.EqualTo("test"));
        }

        [Test]
        public void CurrentDirectory()
        {
            var fs = new VirtualFileSystem();

            Assert.That(fs.GetCurrentDirectory(), Is.EqualTo("home"));
        }

        [Test]
        public void EnumeratingDirectories()
        {
            var fs = new VirtualFileSystem();

            fs.CreateDirectory("foo/bar/1/a");
            fs.CreateDirectory("foo/bar/1/b");

            var bar = fs.GetDirectory("foo/bar");

            var dirs = bar.EnumerateDirectories().First().EnumerateDirectories().ToList();

            Assert.That(dirs[0].Name, Is.EqualTo("a"));
            Assert.That(dirs[1].Name, Is.EqualTo("b"));
        }
    }
}
