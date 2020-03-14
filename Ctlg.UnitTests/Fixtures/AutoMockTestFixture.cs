using System;
using Autofac;
using Autofac.Extras.Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests.Fixtures
{
    public abstract class AutoMockTestFixture : BaseTestFixture
    {
        [SetUp]
        public void SetupAutoMock()
        {
            AutoMock = AutoMock.GetLoose(ConfigureDependencies);
        }

        [TearDown]
        public void TearDownAutoMock()
        {
            AutoMock.Dispose();
        }

        protected virtual void ConfigureDependencies(ContainerBuilder builder)
        {

        }

        protected AutoMock AutoMock;
    }
}
