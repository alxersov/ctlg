using System;
using System.IO;
using System.Linq;
using Autofac.Extras.Moq;
using Ctlg.Service;
using Ctlg.Service.Utils;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests.Services
{
    public class ArchiveServiceTests
    {
        [TestCase(@"foo/bar.zip")]
        [TestCase(@"bar.Rar")]
        [TestCase(@"1.7z")]
        public void IsArchiveExtension_WhenIsSupportedArchive_ReturnsTrue(string path)
        {
            using (var mock = AutoMock.GetLoose())
            {
                var service = mock.Create<ArchiveService>();

                Assert.That(service.IsArchiveExtension(path), Is.True);
            }
        }

        [TestCase(@"foo/bar.zip1")]
        [TestCase(@"barRar")]
        [TestCase(@"1.7z/test")]
        [TestCase(@"test/zip")]
        public void IsArchiveExtension_WhenIsNotSupportedArchive_ReturnsFalse(string path)
        {
            using (var mock = AutoMock.GetLoose())
            {
                var service = mock.Create<ArchiveService>();

                Assert.That(service.IsArchiveExtension(path), Is.False);
            }
        }

        [Test]
        public void OpenArchive_CreatesArchive()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var service = mock.Create<ArchiveService>();

                var zip = string.Join("", new[] {
                    "504b03040a00000000008c66ec4e000000000000000000000000020000006869",
                    "504b01021e030a00000000008c66ec4e00000000000000000000000002000000",
                    "0000000000000000a481000000006869504b0506000000000100010030000000",
                    "200000000000" });
                var bytes = FormatBytes.ToByteArray(zip);
                var stream = new MemoryStream(bytes);
                var archive = service.OpenArchive(stream);

                var files = archive.EnumerateEntries().ToList();

                Assert.That(files[0].Name, Is.EqualTo("hi"));
            }
        }
    }
}
