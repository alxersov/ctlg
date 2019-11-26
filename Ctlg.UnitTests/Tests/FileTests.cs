using System;
using Ctlg.Core;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests
{
    [TestFixture]
    public class FileTests
    {
        [Test]
        public void BuildFullPath_WhenParentIsNotLoaded_ThrowsException()
        {
            var file = new File
            {
                Name = "a",
                ParentFileId = 1
            };

            Assert.That(() => { file.BuildFullPath(); },
                Throws.TypeOf<InvalidOperationException>()
                    .With.Message.Contain("ParentFile is not loaded"));
        }
    }
}
