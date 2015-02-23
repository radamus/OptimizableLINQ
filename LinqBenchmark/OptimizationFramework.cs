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

        public override string ToString()
        {
            return "OptimizableLINQ";
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

        public override string ToString()
        {
            return "PLINQ";
        }
    }

    public class OptimizationCompositionApplicator : OptimizationApplicator
    {
        IList<OptimizationApplicator> optApps = new List<OptimizationApplicator>();
        
        public OptimizationCompositionApplicator(params OptimizationApplicator[] applicators) {
            foreach (OptimizationApplicator app in applicators)
                optApps.Add(app);
        }

        public Expression GetOptimizedExpression<TSource>(Func<IQueryable<TSource>, IQueryable> queryFunc, IEnumerable<TSource> source)
        {
            return Apply<TSource>(queryFunc, source).Expression;
        }

        public IQueryable<TSource> Apply<TSource>(IEnumerable<TSource> source)
        {
            foreach (OptimizationApplicator app in optApps)
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

        public override string ToString()
        {
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < optApps.Count(); i++)
            {
                res.Append(optApps[i].ToString());
                if (i < optApps.Count() - 2)
                    res.Append(", ");
                else if (i == optApps.Count() - 2)
                    res.Append(" and ");
            }

            return res.ToString();
        }
    }

}
