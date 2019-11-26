using System;
using System.Collections.Generic;
using Ctlg.Service;
using NUnit.Framework;

namespace Ctlg.UnitTests.Fixtures
{
    [TestFixture]
    public abstract class BaseTestFixture
    {
        [TearDown]
        public void ClearEventHandlers()
        {
            DomainEvents.ClearCallbacks();
        }

        /// <summary>
        /// Sets up domain events: gathers events of type T into the list.
        /// </summary>
        /// <returns>The list object that will contain raised events.</returns>
        /// <typeparam name="T">Any IDomainEvent</typeparam>
        protected static IList<T> SetupEvents<T>() where T : IDomainEvent
        {
            var events = new List<T>();
            DomainEvents.Register<T>(events.Add);
            return events;
        }
    }
}
