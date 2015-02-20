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

    public class EnumerableSourceCardinalityManagement<TSource> : IQuerySourceCardinalityManagement<IEnumerable<TSource>>
    {
        
        IEnumerable<TSource> fullSource;

        public EnumerableSourceCardinalityManagement(IEnumerable<TSource> source) {
            fullSource = source;
        }

        int IQuerySourceCardinalityManagement<IEnumerable<TSource>>.GetFullCardinality() {
            return fullSource.Count();
        }

        void IQuerySourceCardinalityManagement<IEnumerable<TSource>>.Reduce(ref IEnumerable<TSource> source, int reducedCollectionCount) {
            source = fullSource.Take(reducedCollectionCount).ToList();
        }

        void IQuerySourceCardinalityManagement<IEnumerable<TSource>>.Revert(ref IEnumerable<TSource> source) {
            source = fullSource;
        }
    }

    public class IntCardinalityManagementStrategy : IQuerySourceCardinalityManagement<int>
    {
        int fullCount;

        public IntCardinalityManagementStrategy(int count)
        {
            fullCount = count;
        }

        int IQuerySourceCardinalityManagement<int>.GetFullCardinality() {
            return fullCount;
        }

        void IQuerySourceCardinalityManagement<int>.Reduce(ref int count, int reducedCollectionCount) {
            count = reducedCollectionCount;
        }

        void IQuerySourceCardinalityManagement<int>.Revert(ref int count) {
            count = fullCount;
        }

    }
}