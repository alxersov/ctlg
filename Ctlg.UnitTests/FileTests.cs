using System;
using Ctlg.Data.Model;
using NUnit.Framework;

namespace Ctlg.UnitTests
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
