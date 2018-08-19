using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SimpleGame.Helpers
{
    public class ObservablePairCollection<TKey, TValue> : ObservableCollection<Pair<TKey, TValue>>
    {
        public ObservablePairCollection()
            : base()
        {
        }

        public ObservablePairCollection(IEnumerable<Pair<TKey, TValue>> enumerable)
            : base(enumerable)
        {
        }

        public ObservablePairCollection(List<Pair<TKey, TValue>> list)
            : base(list)
        {
        }

        public ObservablePairCollection(IDictionary<TKey, TValue> dictionary)
        {
            foreach (var kv in dictionary)
            {
                Add(new Pair<TKey, TValue>(kv));
            }
        }

        public void Add(TKey key, TValue value)
        {
            Add(new Pair<TKey, TValue>(key, value));
        }
    }
}
