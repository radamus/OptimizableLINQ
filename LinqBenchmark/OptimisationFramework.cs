using OptimisableLINQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OptimisableLINQBenchmark
{
    public interface OptimisationApplicator
    {
        Expression GetOptimisedExpression<TSource>(Func<IQueryable<TSource>, IQueryable> queryFunc, IEnumerable<TSource> source);

        IQueryable<TSource> Apply<TSource>(IEnumerable<TSource> source);

        IQueryable Compile(IQueryable optQuery);

        IQueryable Apply<TSource>(Func<IQueryable<TSource>, IQueryable> queryFunc, IEnumerable<TSource> source);
    }

    public class OptimizableLINQApplicator: OptimisationApplicator
    {
        Expression OptimisationApplicator.GetOptimisedExpression<TSource>(Func<IQueryable<TSource>, IQueryable> queryFunc, IEnumerable<TSource> source)
        {
            return new Rewriter().Optimize((this as OptimisationApplicator).Apply<TSource>(queryFunc, source).Expression);
        }

        IQueryable<TSource> OptimisationApplicator.Apply<TSource>(IEnumerable<TSource> source)
        {
            return source.AsOptimizable();
        }

        IQueryable OptimisationApplicator.Compile(IQueryable optQuery)
        {
            optQuery.GetEnumerator();
            return optQuery;
        }

        IQueryable OptimisationApplicator.Apply<TSource>(Func<IQueryable<TSource>, IQueryable> queryFunc, IEnumerable<TSource> source)
        {
            return queryFunc((this as OptimisationApplicator).Apply<TSource>(source));
        }

    }



}
