using System;
using System.Collections.Generic;
using System.Linq;

namespace linkedin_techscreen
{

    public interface Rankable
    {
        /* determined by an external system */
        long getRank();
    }

    public interface DataSource<K, V>
        where V : Rankable
    {
        V get(K key);
    }

    public class RetainBestCache<K, V> : DataSource<K, V>
        where V : Rankable
    {

        private DataSource<K, V> dataSource;
        private Dictionary<K, V> cache;
        private SortedList<long, (K, V)> byRank;
        private int maxItems;
        private object lockObj;

        RetainBestCache(DataSource<K, V> dataSource, int maxItems)
        {
            this.dataSource = dataSource;
            this.cache = new Dictionary<K, V>();
            this.byRank = new SortedList<long, (K, V)>();
            this.maxItems = maxItems;
            this.lockObj = new object();
        }

        /**
         * If in cache, return it.
         * If not in cache, fetch from datasource, caching result.
         * If cache is full, purge item with lowest rank.
         * If there multiple items with the same lowest rank, purge which ever on you
         * feel like.
         */

        /*
         * lock(object instance) {
         *   thing
         * }
         */

        public V get(K key)
        {
            lock (lockObj)
            {
                if (cache.ContainsKey(key))
                {
                    return cache[key];
                }
            }

            var v = dataSource.get(key);
            if (v == null)
            {
                return default(V);
            }

            lock (lockObj)
            {
                if (cache.Count() >= maxItems)
                {
                    var (k, _) = byRank[0]
                    byRank.RemoveAt(0);
                    cache.Remove(k);
                }
                byRank.Add(v.getRank(), (key, v));
                cache.Add(key, v);

                return v;
            }
        }
    }

    public class LongRankable : Rankable
    {
        public long Value { get; }
        public LongRankable(long value)
        {
            this.Value = value;
        }
        public long getRank()
        {
            return Value;
        }
    }

    public class DictDataSource<K, V> : DataSource<K, V>
        where V : Rankable
    {
        private Dictionary<K, V> data;
        public DictDataSource() {
            data = new Dictionary<K, V>();
        }
        public V get(K key)
        {
            if(!data.ContainsKey(key))
                return default(V);
            return data[key];
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            
        }
    }
}
