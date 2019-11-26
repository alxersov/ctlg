using Ctlg.Core.Interfaces;
using Ctlg.Service.Commands;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests.Commands
{
    [TestFixture]
    public class ListCommandTests
    {
        [Test]
        public void Execute_WhenCalled_CallsListFiles()
        {

            var serviceMock = new Mock<ICtlgService>(MockBehavior.Strict);
            serviceMock.Setup(s => s.ListFiles());

            var command = new ListCommand(serviceMock.Object);

            command.Execute();

            serviceMock.VerifyAll();
        }
    }
}
