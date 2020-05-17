using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Extras.Moq;
using Ctlg.Service.Events;
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

        [SetUp]
        public void SetupErrorEvents()
        {
            Errors = SetupEvents<ErrorEvent>();
        }

        [TearDown]
        public void TearDownAutoMock()
        {
            AutoMock.Dispose();
        }

        protected virtual void ConfigureDependencies(ContainerBuilder builder)
        {

        }

        protected IList<ErrorEvent> Errors { get; set; }

        protected AutoMock AutoMock;
    }
}
