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
            Assert.That(command, Is.InstanceOf<AddCommand>());

            var addCommand = (AddCommand) command;
            Assert.That(addCommand.Path, Is.EqualTo("some path"));
        }

        [Test]
        public void Parse_WhenListCommandWithoutParameters_RetrusnListCommand()
        {
            var parser = new ArgsParser();
            var command = parser.Parse(new[] {"list"});
            Assert.That(command, Is.InstanceOf<ListCommand>());
        }

        [Test]
        public void Parse_WhenFindCommandWithArugment_ReturnsFindCommand()
        {
            var parser = new ArgsParser();
            var command = parser.Parse(new[] {"find", "01ff"});
            Assert.That(command, Is.InstanceOf<FindCommand>());
            var findCommand = (FindCommand) command;
            Assert.That(findCommand.Hash, Is.EqualTo("01ff"));
        }
    }
}
