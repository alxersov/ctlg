using System.Collections.Generic;
using Autofac.Features.Indexed;

namespace Ctlg.UnitTests
{
    public class Index<TKey, TValue> : IIndex<TKey, TValue>
    {
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get { return _dictionary[key]; }
        }

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
        }

        private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
    }
}
