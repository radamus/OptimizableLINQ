using System;
using System.Collections;
using System.Linq;
using System.Text;
using OptimisableLINQ;

namespace OptimisableLINQBenchmark
{
    public interface IQueryExecutor<TQuery>
    {
        // Forces query compilation for queryable and optimizable queries and warmup.
        void Prepare(ref TQuery query);

        void Run(ref TQuery query);

        int ResultCRC(TQuery query);
    }

    public class EnumerableQueryExecutor : IQueryExecutor<IEnumerable>
    {
        public void Prepare(ref IEnumerable query)
        {
            query.GetEnumerator();
        }

        public void Run(ref IEnumerable query)
        {
            foreach (object item in query)
            {
            }
        }

        public int ResultCRC(IEnumerable query)
        {
            return query.Count();
        }
    }

    public class FuncEnumerableQueryExecutor : IQueryExecutor<Func<IEnumerable>>
    {
        public void Prepare(ref Func<IEnumerable> query)
        {
            query().GetEnumerator();
        }

        public void Run(ref Func<IEnumerable> query)
        {
            foreach (object item in query.Invoke())
            {
            }
        }

        public int ResultCRC(Func<IEnumerable> query)
        {
            return query.Invoke().Count();
        }
    }

    public class FuncIntQueryExecutor: IQueryExecutor<Func<int>>
    {
        public void Prepare(ref Func<int> query)
        {
            query();
        }

        public void Run(ref Func<int> query)
        {
            query();
        }

        public int ResultCRC(Func<int> query)
        {
            return query();
        }
    }
}
