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
        void prepare(ref TQuery query);

        void run(ref TQuery query);

        int resultCRC(TQuery query);
    }

    public class EnumerableQueryExecutor : IQueryExecutor<IEnumerable>
    {
        public void prepare(ref IEnumerable query)
        {
            query.GetEnumerator();
        }

        public void run(ref IEnumerable query)
        {
            foreach (object item in query)
            {
            }
        }

        public int resultCRC(IEnumerable query)
        {
            return query.Count();
        }
    }

    public class FuncEnumerableQueryExecutor : IQueryExecutor<Func<IEnumerable>>
    {
        public void prepare(ref Func<IEnumerable> query)
        {
            query().GetEnumerator();
        }

        public void run(ref Func<IEnumerable> query)
        {
            foreach (object item in query.Invoke())
            {
            }
        }

        public int resultCRC(Func<IEnumerable> query)
        {
            return query.Invoke().Count();
        }
    }

    public class FuncIntQueryExecutor: IQueryExecutor<Func<int>>
    {
        public void prepare(ref Func<int> query)
        {
            query();
        }

        public void run(ref Func<int> query)
        {
            query();
        }

        public int resultCRC(Func<int> query)
        {
            return query();
        }
    }
}
