using Autofac.Extras.Moq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.Commands;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests.Commands
{
    [TestFixture]
    public class AddCommandTests
    {
        [Test]
        public void Execute_WhenCalled_CallsAddDirectory()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var tree = new File("test-path", true) { Contents = { new File("test-1.txt") } };

                var hashFunctionMock = new Mock<IHashFunction>();

                mock.Mock<ITreeProvider>()
                    .Setup(d => d.ReadTree(It.Is<string>(s => s == "test-path"), It.Is<string>(s => s == null)))
                    .Returns(tree);

                var command = mock.Create<AddCommand>();
                command.Path = "test-path";

                command.Execute();

                mock.Mock<IDataService>().Verify(s => s.AddDirectory(tree), Times.Once);
            }
        }
    }
}
