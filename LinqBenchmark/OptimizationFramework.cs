using OptimizableLINQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OptimizableLINQBenchmark
{
    public interface OptimizationApplicator
    {
        Expression GetOptimizedExpression<TSource>(Func<IQueryable<TSource>, IQueryable> queryFunc, IEnumerable<TSource> source);

        IQueryable<TSource> Apply<TSource>(IEnumerable<TSource> source);

        IQueryable Compile(IQueryable optQuery);

        IQueryable Apply<TSource>(Func<IQueryable<TSource>, IQueryable> queryFunc, IEnumerable<TSource> source);
    }

    public class OptimizableLINQApplicator: OptimizationApplicator
    {
        Expression OptimizationApplicator.GetOptimizedExpression<TSource>(Func<IQueryable<TSource>, IQueryable> queryFunc, IEnumerable<TSource> source)
        {
            return new Rewriter().Optimize((this as OptimizationApplicator).Apply<TSource>(queryFunc, source).Expression);
        }

        IQueryable<TSource> OptimizationApplicator.Apply<TSource>(IEnumerable<TSource> source)
        {
            return source.AsOptimizable();
        }

        IQueryable OptimizationApplicator.Compile(IQueryable optQuery)
        {
            optQuery.GetEnumerator();
            return optQuery;
        }

        IQueryable OptimizationApplicator.Apply<TSource>(Func<IQueryable<TSource>, IQueryable> queryFunc, IEnumerable<TSource> source)
        {
            return queryFunc((this as OptimizationApplicator).Apply<TSource>(source));
        }
    }

    public class ParallelLINQApplicator : OptimizationApplicator
    {
        Expression OptimizationApplicator.GetOptimizedExpression<TSource>(Func<IQueryable<TSource>, IQueryable> queryFunc, IEnumerable<TSource> source)
        {
            return (this as OptimizationApplicator).Apply<TSource>(queryFunc, source).Expression;
        }

        IQueryable<TSource> OptimizationApplicator.Apply<TSource>(IEnumerable<TSource> source)
        {
            return (IQueryable<TSource>)source.AsParallel().AsQueryable();
        }

        IQueryable OptimizationApplicator.Compile(IQueryable optQuery)
        {
            optQuery.GetEnumerator();
            return optQuery;
        }

        IQueryable OptimizationApplicator.Apply<TSource>(Func<IQueryable<TSource>, IQueryable> queryFunc, IEnumerable<TSource> source)
        {
            return queryFunc((this as OptimizationApplicator).Apply<TSource>(source));
        }
    }

    public class OptimizationCompositionApplicator : OptimizationApplicator
    {
        IList<OptimizationApplicator> optApp = new List<OptimizationApplicator>();
        
        public OptimizationCompositionApplicator(params OptimizationApplicator[] applicators) {
            foreach (OptimizationApplicator app in applicators)
                optApp.Add(app);
        }

        Expression OptimizationApplicator.GetOptimizedExpression<TSource>(Func<IQueryable<TSource>, IQueryable> queryFunc, IEnumerable<TSource> source)
        {
            return (this as OptimizationApplicator).Apply<TSource>(queryFunc, source).Expression;
        }

        IQueryable<TSource> OptimizationApplicator.Apply<TSource>(IEnumerable<TSource> source)
        {
            foreach (OptimizationApplicator app in optApp)
                source = app.Apply(source);
            
            return (IQueryable<TSource>) source;
        }

        IQueryable OptimizationApplicator.Compile(IQueryable optQuery)
        {
            optQuery.GetEnumerator();
            return optQuery;
        }

        IQueryable OptimizationApplicator.Apply<TSource>(Func<IQueryable<TSource>, IQueryable> queryFunc, IEnumerable<TSource> source)
        {
            return queryFunc((this as OptimizationApplicator).Apply<TSource>(source));
        }
    }

}
