using System;
using Ctlg.Service.Utils;
using NUnit.Framework;

namespace Ctlg.UnitTests
{
    [TestFixture]
    public class FileSizeTests
    {
        [TestCase(0, "0")]
        [TestCase(1, "1")]
        [TestCase(1023, "1023")]
        [TestCase(1024, "1k")]
        [TestCase(1025, "1.001k")]
        [TestCase(1536, "1.5k")]
        [TestCase(1024 * 1024, "1M")]
        [TestCase(1024 * 1024 * 1024, "1G")]
        [TestCase(1024L * 1024 * 1024 * 1024, "1T")]
        [TestCase(1024L * 1024 * 1024 * 1024 * 1024, "1P")]
        public void Format(long size, string expectedString)
        {
            var actual = FileSize.Format(size);

            Assert.That(actual, Is.EqualTo(expectedString));
        }
    }
}
