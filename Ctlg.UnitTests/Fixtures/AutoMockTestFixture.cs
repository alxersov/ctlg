using System;
using Autofac.Extras.Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests.Fixtures
{
    public abstract class AutoMockTestFixture : BaseTestFixture
    {
        [SetUp]
        public void SetupAutoMock()
        {
            AutoMock = AutoMock.GetLoose();
        }

        [TearDown]
        public void TearDownAutoMock()
        {
            AutoMock.Dispose();
        }

        protected AutoMock AutoMock;
    }
}
