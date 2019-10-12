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
            var service = new IndexService(1);

            service.Add(new byte[] { 0x2 });
            service.Add(new byte[] { 0x1 });

            Assert.That(service.GetAllHashes(), Is.EquivalentTo(new[] { new byte[] { 0x1 }, new byte[] { 0x2 } }));
        }

        [Test]
        public void Add_WhenHashHasUnexpectedLength_ThrowsException()
        {
            var service = new IndexService(1);
            var hash = new byte[] { 1, 2 };

            Assert.That(() => service.Add(hash),
                Throws.InstanceOf<Exception>()
                    .With.Message.Contain("Expected hash to have lenght 1 bytes"));
        }
    }
}
