using System;
using Ctlg.Service;
using NUnit.Framework;

namespace Ctlg.UnitTests
{
    [TestFixture]
    public class IndexServiceTests
    {
        [Test]
        public void GetAllHashes_Returns_PreviouslyAddedHashes()
        {
            var service = new IndexService();

            service.Add(new byte[] { 0x2 });
            service.Add(new byte[] { 0x1 });

            Assert.That(service.GetAllHashes(), Is.EquivalentTo(new[] { new byte[] { 0x1 }, new byte[] { 0x2 } }));
        }
    }
}
