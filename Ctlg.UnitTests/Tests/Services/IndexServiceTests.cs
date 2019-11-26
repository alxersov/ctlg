using System;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Services;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests.Services
{
    [TestFixture]
    public class IndexServiceTests
    {
        private IIndexService IndexService;
        private byte[] Hash1 = { 0x1 };
        private byte[] Hash2 = { 0x2 };
        private byte[] HashNotInIndex = { 0x3 };
        private byte[] HashOtherLength = { 1, 2 };

        [SetUp]
        public void Setup()
        {
            IndexService = new IndexService(1);

            IndexService.Add(Hash2);
            IndexService.Add(Hash1);
        }

        [Test]
        public void GetAllHashes_Returns_PreviouslyAddedHashes()
        {
            Assert.That(IndexService.GetAllHashes(), Is.EquivalentTo(new[] { Hash1, Hash2 }));
        }

        [Test]
        public void Add_WhenHashHasUnexpectedLength_ThrowsException()
        {
            Assert.That(() => IndexService.Add(HashOtherLength),
                Throws.InstanceOf<Exception>()
                    .With.Message.Contain("Expected hash to have length 1 bytes"));
        }

        [Test]
        public void IsInIndex_WhenHashFound_ReturnsTrue()
        {
            Assert.That(IndexService.IsInIndex(Hash1), Is.True);
        }

        [Test]
        public void IsInIndex_WhenHashNotFound_ReturnsFalse()
        {
            Assert.That(IndexService.IsInIndex(HashNotInIndex), Is.False);
        }
    }
}
