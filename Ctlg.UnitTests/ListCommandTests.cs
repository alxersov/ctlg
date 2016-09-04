using Ctlg.Service;
using Ctlg.Service.Commands;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests
{
    [TestFixture]
    public class ListCommandTests
    {
        [Test]
        public void Execute_WhenCalled_CallsListFiles()
        {
            var command = new ListCommand();

            var serviceMock = new Mock<ICtlgService>(MockBehavior.Strict);
            serviceMock.Setup(s => s.ListFiles());

            command.Execute(serviceMock.Object);

            serviceMock.VerifyAll();
        }
    }
}
