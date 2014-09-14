using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SBQL
{
    public static class Sbqlable
    {
        internal class EmptyResult<T> : IEnumerable<T>
        {
            EmptyResult()
            {
            }
            T[] elem = new T[0];

            public IEnumerator<T> GetEnumerator()
            {
                return (IEnumerator<T>)elem.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return elem.GetEnumerator();
            }

            internal T[] Elem { get { return elem; } }
            internal static readonly EmptyResult<T> Empty = new EmptyResult<T>();
        }

        internal static TResult Eval<TSource, TQ2Result, TPartialResult, TResult>(IEnumerable<TSource> source, Func<TSource, IEnumerable<TQ2Result>> evalQ2, 
            Func<TSource, IEnumerable<TQ2Result>, IEnumerable<TPartialResult>> partialResultOf, Func<IEnumerable<TPartialResult>, TResult> mergePartialResult)
        {
            List<TPartialResult> partialResults = new List<TPartialResult>();
            foreach (TSource e in source)
            {
                IEnumerable<TQ2Result> res = evalQ2(e);
                partialResults.AddRange(partialResultOf(e, res));
            }

            return mergePartialResult(partialResults);
        }

        internal static TResult Eval<TSource, TQ2Result, TPartialResult, TResult>(IQueryable<TSource> source, Expression<Func<TSource, IQueryable<TQ2Result>>> evalQ2, Expression<Func<TSource, IEnumerable<TQ2Result>, IEnumerable<TPartialResult>>> partialResultOf, Expression<Func<IEnumerable<TPartialResult>, TResult>> mergePartialResult)
        {

            if (typeof(TResult).Name == typeof(IQueryable<>).Name)
            {
                Type queryableType = typeof(TResult).GetGenericArguments()[0];
                return (TResult)source.Provider.CreateQuery(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(queryableType, typeof(TQ2Result), typeof(TPartialResult), typeof(TResult)),
                    new Expression[] { source.Expression, Expression.Quote((Expression)evalQ2), Expression.Quote((Expression)partialResultOf), Expression.Quote((Expression)mergePartialResult) }));
            }
            else
            {
                return source.Provider.Execute<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TQ2Result), typeof(TPartialResult), typeof(TResult)),
                    new Expression[] { source.Expression, Expression.Quote((Expression)evalQ2), Expression.Quote((Expression)partialResultOf), Expression.Quote((Expression)mergePartialResult) }));
            }
            
        }

        /// <summary>
        /// Version of eval for operators that requires single value as a Q2 result
        /// this is redundant method increasing performancy - operator implementation can avoid one lambda
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TQ2Result"></typeparam>
        /// <typeparam name="TPartialResult"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="evalQ2"></param>
        /// <param name="partialResultOf"></param>
        /// <param name="mergePartialResult"></param>
        /// <returns></returns>
        internal static TResult EvalSingle<TSource, TQ2Result, TPartialResult, TResult>(IEnumerable<TSource> source, Func<TSource, TQ2Result> evalQ2, Func<TSource, TQ2Result, IEnumerable<TPartialResult>> partialResultOf, Func<IEnumerable<TPartialResult>, TResult> mergePartialResult)
        {
            List<TPartialResult> partialResults = new List<TPartialResult>();
            foreach (TSource e in source)
            {
                TQ2Result res = evalQ2(e);
                partialResults.AddRange(partialResultOf(e, res));
            }

            return mergePartialResult(partialResults);
        }

        internal static TResult EvalSingle<TSource, TQ2Result, TPartialResult, TResult>(IQueryable<TSource> source, Func<TSource, TQ2Result> evalQ2, Func<TSource, TQ2Result, IEnumerable<TPartialResult>> partialResultOf, Func<IEnumerable<TPartialResult>, TResult> mergePartialResult)
        {
            List<TPartialResult> partialResults = new List<TPartialResult>();
            foreach (TSource e in source)
            {
                TQ2Result res = evalQ2(e);
                partialResults.AddRange(partialResultOf(e, res));
            }

            return mergePartialResult(partialResults);
        }

        internal static IEnumerable<TResult> LazyEval<TSource, TQ2Result, TResult>(IEnumerable<TSource> source, Func<TSource, IEnumerable<TQ2Result>> evalQ2, Func<TSource, IEnumerable<TQ2Result>, IEnumerable<TResult>> partialResultOf)
        {
            foreach (TSource e in source)
            {
                IEnumerable<TQ2Result> res = evalQ2(e);
                foreach (TResult pres in partialResultOf(e, res))
                    yield return pres;
            }
        }
        /// <summary>
        /// Version eval for operators that requires single value as q2 result - automatic coerce toSingle
        /// this is redundant method increasing performancy - operator implementation can avoid one lambda
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TQ2Result"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="evalQ2"></param>
        /// <param name="partialResultOf"></param>
        /// <returns></returns>
        internal static IEnumerable<TResult> LazyEvalSingle<TSource, TQ2Result, TResult>(IEnumerable<TSource> source, Func<TSource, TQ2Result> evalQ2, Func<TSource, TQ2Result, IEnumerable<TResult>> partialResultOf)
        {
            foreach (TSource e in source)
            {
                TQ2Result res = evalQ2(e);
                foreach (TResult pres in partialResultOf(e, res))
                    yield return pres;
            }
        }

        public static IEnumerable<TSource> SBQLWhere<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            //return LazyEval(source, s => new bool[] { predicate(s) }, (e, q2r) => q2r.First() == true ? new TSource[] { e } : EmptyResult<TSource>.Empty.Elem);
            return LazyEvalSingle(source, predicate, (e, q2r) => q2r ? new TSource[] { e } : EmptyResult<TSource>.Empty.Elem);
        }

        public static IEnumerable<TResult> Navi<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> navigator)
        {
            return LazyEval(source, e => new TResult[] { navigator(e) }, (e, q2r) => q2r);
        }

        public static IEnumerable<TResult> NaviMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> navigator)
        {
            return LazyEval(source, navigator, (e, q2r) => q2r);
        }

        public static IEnumerable<TResult> Join<TSource, TJResult, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TJResult>> inner, Func<TSource, TJResult, TResult> resultCreator)
        {
            return LazyEval(source, inner, (e, q2r) => createResult(e, q2r, resultCreator));
        }

        static IEnumerable<TResult> createResult<T1, T2, TResult>(T1 v1, IEnumerable<T2> v2, Func<T1, T2, TResult> resultCreator)
        {
            foreach (T2 ev2 in v2)
            {
                yield return resultCreator(v1, ev2);
            }
        }
        public static bool ForAny<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> condition)
        {
            //return Eval(source, p => new bool[] { condition(p) }, (e, q2r) => q2r.First() == true ? new bool[] { true } : EmptyResult<bool>.Empty.Elem, pr => pr.Any() ? true : false);
            return EvalSingle(source, condition, (e, q2r) => q2r ? new bool[] { true } : EmptyResult<bool>.Empty.Elem, pr => pr.Any() ? true : false);
        }

        public static bool ForSome<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> condition)
        {
            //return Eval(source, p => new bool[] { condition(p) }, (e, q2r) => q2r.First() == false ? new bool[] { false } : EmptyResult<bool>.Empty.Elem, pr => pr.Any() ? false : true);
            return EvalSingle(source, condition, (e, q2r) => !q2r  ? new bool[] { false } : EmptyResult<bool>.Empty.Elem, pr => pr.Any() ? false : true);
        }


        public static IEnumerable<TSource> CloseBy<TSource>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> close)
        {
            Queue<IEnumerable<TSource>> queue = new Queue<IEnumerable<TSource>>();
            queue.Enqueue(source);
            List<TSource> finalResult = new List<TSource>();
            while (queue.Count > 0)
            {
                IEnumerable<TSource> current = queue.Dequeue();
                IEnumerable<TSource> res = Eval(current, close, (e, p) => p, p=>p);
                if (res.Count() > 0)
                {
                    finalResult.AddRange(res);
                    queue.Enqueue(res);
                }
            }
            return finalResult;
        }

        //IEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TKey>> keySelector, IEqualityComparer<TKey> comparer)
        //{
        //    return Eval(source, p => p, (e, p) => p , pr => Sorter(pr, keySelector, comparer));
        //}

    

    }
       
}
