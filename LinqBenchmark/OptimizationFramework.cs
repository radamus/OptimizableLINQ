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
        public Expression GetOptimizedExpression<TSource>(Func<IQueryable<TSource>, IQueryable> queryFunc, IEnumerable<TSource> source)
        {
            return new Rewriter().Optimize(Apply<TSource>(queryFunc, source).Expression);
        }

        public IQueryable<TSource> Apply<TSource>(IEnumerable<TSource> source)
        {
            return source.AsOptimizable();
        }

        public IQueryable Compile(IQueryable optQuery)
        {
            optQuery.GetEnumerator();
            return optQuery;
        }

        public IQueryable Apply<TSource>(Func<IQueryable<TSource>, IQueryable> queryFunc, IEnumerable<TSource> source)
        {
            return queryFunc(Apply<TSource>(source));
        }
    }

    public class ParallelLINQApplicator : OptimizationApplicator
    {
        public Expression GetOptimizedExpression<TSource>(Func<IQueryable<TSource>, IQueryable> queryFunc, IEnumerable<TSource> source)
        {
            return Apply<TSource>(queryFunc, source).Expression;
        }

        public IQueryable<TSource> Apply<TSource>(IEnumerable<TSource> source)
        {
            return (IQueryable<TSource>)source.AsParallel().AsQueryable();
        }

        public IQueryable Compile(IQueryable optQuery)
        {
            optQuery.GetEnumerator();
            return optQuery;
        }

        public IQueryable Apply<TSource>(Func<IQueryable<TSource>, IQueryable> queryFunc, IEnumerable<TSource> source)
        {
            return queryFunc(Apply<TSource>(source));
        }
    }

    public class OptimizationCompositionApplicator : OptimizationApplicator
    {
        IList<OptimizationApplicator> optApp = new List<OptimizationApplicator>();
        
        public OptimizationCompositionApplicator(params OptimizationApplicator[] applicators) {
            foreach (OptimizationApplicator app in applicators)
                optApp.Add(app);
        }

        public Expression GetOptimizedExpression<TSource>(Func<IQueryable<TSource>, IQueryable> queryFunc, IEnumerable<TSource> source)
        {
            return Apply<TSource>(queryFunc, source).Expression;
        }

        public IQueryable<TSource> Apply<TSource>(IEnumerable<TSource> source)
        {
            foreach (OptimizationApplicator app in optApp)
                source = app.Apply(source);
            
            return (IQueryable<TSource>) source;
        }

        public IQueryable Compile(IQueryable optQuery)
        {
            optQuery.GetEnumerator();
            return optQuery;
        }

        public IQueryable Apply<TSource>(Func<IQueryable<TSource>, IQueryable> queryFunc, IEnumerable<TSource> source)
        {
            return queryFunc(Apply<TSource>(source));
        }
    }

}
