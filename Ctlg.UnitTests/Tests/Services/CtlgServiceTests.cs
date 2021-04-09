using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using Autofac.Extras.Moq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Filesystem;
using Ctlg.Service.Commands;
using Ctlg.Service.Events;
using Ctlg.Service.Services;
using Ctlg.UnitTests.Fixtures;
using Moq;
using NUnit.Framework;
using File = Ctlg.Core.File;

namespace Ctlg.UnitTests.Tests.Services
{
    public class CtlgServiceTests: BaseTestFixture
    {
        [Test]
        public void ApplyDbMigrations_CallsDataService()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var ctlg = mock.Create<CtlgService>();

                ctlg.ApplyDbMigrations();

                mock.Mock<IDataService>().Verify(s => s.ApplyDbMigrations(), Times.Once);
            }
        }
    }
}
