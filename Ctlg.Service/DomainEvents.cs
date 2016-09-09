using System;
using System.Collections.Generic;
using Autofac;

namespace Ctlg.Service
{
    /// <summary>
    /// http://udidahan.com/2009/06/14/domain-events-salvation/
    /// https://msdn.microsoft.com/en-us/magazine/ee236415.aspx
    /// </summary>
    public static class DomainEvents
    {
        public static ILifetimeScope Container { get; set; }

        [ThreadStatic]
        private static List<Delegate> _actions;

        public static void Register<T>(Action<T> callback) where T : IDomainEvent
        {
            if (_actions == null)
            {
                _actions = new List<Delegate>();
            }
            _actions.Add(callback);
        }

        public static void ClearCallbacks()
        {
            _actions = null;
        }

        public static void Raise<T>(T args) where T : IDomainEvent
        {
            if (Container != null)
            {
                foreach (var handler in Container.Resolve<IEnumerable<IHandle<T>>>())
                {
                    handler.Handle(args);
                }
            }

            if (_actions != null)
            {
                foreach (var action in _actions)
                {
                    var actionOfT = action as Action<T>;
                    actionOfT?.Invoke(args);
                }
            }
        }
    }
}
