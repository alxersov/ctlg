using System;
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
                Hash = "01FF"
            };

            var serviceMock = new Mock<ICtlgService>(MockBehavior.Strict);

            byte[] hashParameter = null;
            serviceMock.Setup(s => s.FindFiles(It.IsAny<byte[]>())).Callback<byte[]>(b => hashParameter = b);

            command.Execute(serviceMock.Object);

            serviceMock.VerifyAll();
            Assert.That(FormatBytes.ToHexString(hashParameter), Is.EqualTo("01ff").IgnoreCase);
        }
    }
}
