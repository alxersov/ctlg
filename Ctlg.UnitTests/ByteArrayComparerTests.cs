using System;
using Ctlg.Service;
using NUnit.Framework;

namespace Ctlg.UnitTests
{
    [TestFixture]
    public class ByteArrayComparerTests
    {
        [TestCase(new byte[] { 0x2 }, new byte[] { 0x0, 0x1 }, -1)]
        [TestCase(new byte[] { 0x0, 0x1 }, new byte[] { 0x2 }, 1)]
        [TestCase(new byte[] { 0x3 }, new byte[] { 0x4 }, -1)]
        [TestCase(new byte[] { 0x4 }, new byte[] { 0x3 }, 1)]
        [TestCase(new byte[] { 0x4 }, new byte[] { 0x4 }, 0)]
        public void Compare(byte[] a1, byte[] a2, int result)
        {
            var comparer = new ByteArrayComparer();

            Assert.That(comparer.Compare(a1, a2), Is.EqualTo(result));
        }
    }
}
