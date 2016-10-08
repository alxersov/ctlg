using Ctlg.Core;
using NUnit.Framework;

namespace Ctlg.UnitTests
{
    [TestFixture]
    public class HashTests
    {
        [Test]
        public void HashIsEqualToItself()
        {
            var a = new Hash(1, new byte[] { 1, 2, 3 });
            var b = a;

            AssertEquals(a, b, true);
            AssertEqualityOperator(a, b, true);
        }

        [Test]
        public void HashIsEqualToAnotherHashWihtTheSameProperties()
        {
            var a = new Hash(1, new byte[] { 1, 2, 3 });
            var b = new Hash(1, new byte[] { 1, 2, 3 });

            AssertEquals(a, b, true);
            AssertEqualityOperator(a, b, true);
        }

        [Test]
        public void TwoNullsAreEqual()
        {
            Hash a = null;
            Hash b = null;

            AssertEqualityOperator(a, b, true);
        }

        [Test]
        public void HashIsNotEqualToNullHash()
        {
            Hash a = new Hash(1, new byte[] { 1, 2, 3 });
            Hash b = null;

            Assert.That(a.Equals(b), Is.False);
            AssertEqualityOperator(a, b, false);
        }

        [Test]
        public void HashIsNotEqualToNullObject()
        {
            Hash a = new Hash(1, new byte[] { 1, 2, 3 });
            object b = null;

            Assert.That(a.Equals(b), Is.False);
        }

        [Test]
        public void HashIsNotEqualToOtherObject()
        {
            var a = new Hash(1, new byte[] { 1, 2, 3 });
            var b = "";

            Assert.That(a.Equals(b), Is.False);;
        }

        [Test]
        public void TwoDifferentHashesAreNotEqual()
        {
            var a = new Hash(1, new byte[] { 1, 2, 3 });

            var b = new Hash(2, new byte[] { 1, 2, 3 });
            var c = new Hash(1, new byte[] { 1, 2, 3, 4 });
            var d = new Hash(1, new byte[] { 1, 200, 3 });

            AssertHashesAreNotEqual(a, b);
            AssertHashesAreNotEqual(a, c);
            AssertHashesAreNotEqual(a, d);
        }

        [Test]
        public void TwoDifferentEqualObjectsHaveTheSameHashCode()
        {
            var a = new Hash(1, new byte[] { 1, 2, 3 });
            var b = new Hash(1, new byte[] { 1, 2, 3 });

            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        private void AssertHashesAreNotEqual(Hash x, Hash y)
        {
            AssertEquals(x, y, false);
            AssertEqualityOperator(x, y, false);
        }
        
        private void AssertEquals(Hash a, Hash b, bool expectedResult)
        {
            Assert.That(a.Equals(b), Is.EqualTo(expectedResult));
            Assert.That(b.Equals(a), Is.EqualTo(expectedResult));
        }

        private void AssertEqualityOperator(Hash a, Hash b, bool expectedResult)
        {
            Assert.That(a == b, Is.EqualTo(expectedResult));
            Assert.That(b == a, Is.EqualTo(expectedResult));

            Assert.That(a != b, Is.EqualTo(!expectedResult));
            Assert.That(b != a, Is.EqualTo(!expectedResult));
        }
    }
}
