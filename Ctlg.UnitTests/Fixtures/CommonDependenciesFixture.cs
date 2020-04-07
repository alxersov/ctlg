using System;
using Autofac;
using Ctlg.Core.Interfaces;
using Ctlg.UnitTests.TestDoubles;

namespace Ctlg.UnitTests.Fixtures
{
    public abstract class CommonDependenciesFixture : AutoMockTestFixture
    {
        protected VirtualFileSystem FS { get; set; }
        protected DataServiceMock DataServiceMock { get; set; }

        protected override void ConfigureDependencies(ContainerBuilder builder)
        {
            builder.RegisterCommonDependencies();

            FS = new VirtualFileSystem();
            DataServiceMock = new DataServiceMock();

            builder.RegisterInstance<IFilesystemService>(FS);
            builder.RegisterInstance<IDataService>(DataServiceMock);
        }
    }
}
