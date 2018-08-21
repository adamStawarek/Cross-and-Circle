using System.Collections.Generic;
using System.ComponentModel;

namespace SimpleGame.Helpers
{
    public class Pair<TKey, TValue> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected TKey _key;
        protected TValue _value;

        public TKey Key
        {
            get => _key;
            set
            {
                if ((_key != null || value == null) && (_key == null || value != null) && _key.Equals(value)) return;
                _key = value;
                NotifyPropertyChanged("Key");
            }
        }

        public TValue Value
        {
            get => _value;
            set
            {
                if ((_value != null || value == null) && (_value == null || value != null) &&
                    (_value == null || _value.Equals(value))) return;
                _value = value;
                NotifyPropertyChanged("Value");
            }
        }

        public Pair()
        {
        }

        public Pair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public Pair(KeyValuePair<TKey, TValue> kv)
        {
            Key = kv.Key;
            Value = kv.Value;
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
