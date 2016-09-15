using Ctlg.Service;
using Ctlg.Service.Commands;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests
{
    [TestFixture]
    public class AddCommandTests
    {
        [Test]
        public void Execute_WhenCalled_CallsAddDirectory()
        {
            var addCommand = new AddCommand {Path = @"foo\bar"};

            var serviceMock = new Mock<ICtlgService>(MockBehavior.Strict);
            serviceMock.Setup(s => s.AddDirectory(It.Is<string>(path => path == @"foo\bar"), It.IsAny<string>()));

            addCommand.Execute(serviceMock.Object);

            serviceMock.VerifyAll();
        }
    }
}
