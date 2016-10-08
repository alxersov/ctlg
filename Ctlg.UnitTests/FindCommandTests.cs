using System;
using Ctlg.Core;
using Ctlg.Service;
using Ctlg.Service.Commands;
using Ctlg.Service.Utils;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests
{
    [TestFixture]
    public class FindCommandTests
    {
        [Test]
        public void Execute_Always_CallsFindFiles()
        {
            var command = new FindCommand
            {
                HashFunctionName = "test",
                Hash = "01FF"
            };

            var serviceMock = new Mock<ICtlgService>(MockBehavior.Strict);

            serviceMock.Setup(s => s.GetHashAlgorithm(It.IsAny<string>()))
                .Returns(new HashAlgorithm {HashAlgorithmId = 1, Name = "test"});

            byte[] hashParameter = null;
            serviceMock.Setup(s => s.FindFiles(
                It.IsAny<Hash>(),
                It.IsAny<long?>(),
                It.IsAny<string>())).Callback<Hash, long?, string>((h, s, n) => hashParameter = h.Value);

            command.Execute(serviceMock.Object);

            serviceMock.VerifyAll();
            Assert.That(FormatBytes.ToHexString(hashParameter), Is.EqualTo("01ff").IgnoreCase);
        }
    }
}
