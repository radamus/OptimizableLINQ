using System;
using System.Collections.Generic;
using System.Linq;

namespace OptimizableLINQBenchmark
{
    public interface IQuerySourceCardinalityManagement<TConf>
    {
        int GetFullCardinality();

        void Reduce(ref TConf cardConf, int reducedCollectionCount);

        void Revert(ref TConf cardConf);
    }

    public static class QueryCardinalityManagementFactory
    {
        public static EnumerableSourceCardinalityManagement<TSource> getManagerFor<TSource>(IEnumerable<TSource> source) { return new EnumerableSourceCardinalityManagement<TSource>(source); }
        public static IntCardinalityManagementStrategy getManagerFor(int count) { return new IntCardinalityManagementStrategy(count); }
    }

    public class EnumerableSourceCardinalityManagement<TSource> : IQuerySourceCardinalityManagement<IEnumerable<TSource>>
    {
        
        IEnumerable<TSource> fullSource;

        public EnumerableSourceCardinalityManagement(IEnumerable<TSource> source) {
            fullSource = source;
        }

        public int GetFullCardinality()
        {
            return fullSource.Count();
        }

        public void Reduce(ref IEnumerable<TSource> source, int reducedCollectionCount)
        {
            source = fullSource.Take(reducedCollectionCount).ToList();
        }

        public void Revert(ref IEnumerable<TSource> source) {
            source = fullSource;
        }
    }

    public class CollectionSourceCardinalityManagement<TSource> : IQuerySourceCardinalityManagement<ICollection<TSource>>
    {

        ICollection<TSource> fullSource;

        public CollectionSourceCardinalityManagement(ICollection<TSource> source)
        {
            fullSource = new List<TSource>();
            foreach(TSource e in source) 
                fullSource.Add(e);
        }

        public int GetFullCardinality()
        {
            return fullSource.Count();
        }

        public void Reduce(ref ICollection<TSource> source, int reducedCollectionCount)
        {
            source.Clear();
            int i = 0;
            foreach (TSource e in fullSource)
            {
                if (i++ == reducedCollectionCount)
                    break;
                source.Add(e);
            }
        }

        public void Revert(ref ICollection<TSource> source)
        {
            source.Clear();
            foreach (TSource e in fullSource)
                source.Add(e);
        }
    }

    public class IntCardinalityManagementStrategy : IQuerySourceCardinalityManagement<int>
    {
        int fullCount;

        public IntCardinalityManagementStrategy(int count)
        {
            fullCount = count;
        }

        public int GetFullCardinality() {
            return fullCount;
        }

        public void Reduce(ref int count, int reducedCollectionCount) {
            count = reducedCollectionCount;
        }

        public void Revert(ref int count) {
            count = fullCount;
        }

    }
}