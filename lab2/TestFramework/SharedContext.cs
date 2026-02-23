using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TestFramework
{
    public static class SharedContext
    {
        private static readonly ConcurrentDictionary<string, object> _data = new ConcurrentDictionary<string, object>();

        public static void Set(string key, object value) => _data[key] = value;
        public static T Get<T>(string key) => (T)_data[key];
        public static void Clear() => _data.Clear();
    }
}