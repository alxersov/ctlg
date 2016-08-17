using Ctlg.Service.Commands;
using NUnit.Framework;

namespace Ctlg.UnitTests
{
    [TestFixture]
    public class ArgsParserTests
    {
        [Test]
        public void Parse_WhenEmptyArgs_ReturnsNull()
        {
            var parser = new ArgsParser();
            var command = parser.Parse(new string[0]);
            Assert.That(command, Is.Null);
        }

        [Test]
        public void Parse_WhenAddCommandWithoutParams_ReturnsNull()
        {
            var parser = new ArgsParser();
            var command = parser.Parse(new[] { "add" });
            Assert.That(command, Is.Null);
        }

        [Test]
        public void Parse_WhenAddCommandWithPath_ReturnsAddCommand()
        {
            var parser = new ArgsParser();
            var command = parser.Parse(new [] {"add", "some path"});
            Assert.That(command, Is.InstanceOf(typeof(AddCommand)));

            var addCommand = command as AddCommand;
            Assert.That(addCommand.Path, Is.EqualTo("some path"));
        }
    }
}
