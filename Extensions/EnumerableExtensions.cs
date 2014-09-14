using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeHelpers
{
    using TestData;
    static class EnumerableExtensions
    {
        #region Generic Non Algebraic
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TQ2Result"></typeparam>
        /// <typeparam name="TPartialResult"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <param name="partialResultOf"></param>
        /// <param name="mergePartialResult"></param>
        /// <returns></returns>
        public static TResult NonAlgebraicLambdaOperator<TSource, TQ2Result, TPartialResult, TResult>(this IEnumerable<TSource> q1, Func<TSource, IEnumerable<TQ2Result>> q2, Func<TSource, TQ2Result, EmptyableResult<TPartialResult>> partialResultOf, Func<IEnumerable<TPartialResult>, TResult> mergePartialResult)
        {
            List<TPartialResult> partialResults = new List<TPartialResult>();
            foreach(TSource e in q1)
            {
                IEnumerable<TQ2Result> q2result = q2(e);
                foreach (TQ2Result q2r in q2result)
                {
                    EmptyableResult<TPartialResult> partialResult = partialResultOf(e, q2r);
                    if (!partialResult.isEmpty())
                        partialResults.Add(partialResult.Value);
                }
            }


            return mergePartialResult(partialResults);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TQ2Result"></typeparam>
        /// <typeparam name="TPartialResult"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <param name="partialResultOf"></param>
        /// <param name="mergePartialResult"></param>
        /// <returns></returns>
        public static TResult NonAlgebraicLambdaOperator<TSource, TQ2Result, TPartialResult, TResult>(this IEnumerable<TSource> q1, Func<TSource, TQ2Result> q2, Func<TSource, TQ2Result, EmptyableResult<TPartialResult>> partialResultOf, Func<IEnumerable<TPartialResult>, TResult> mergePartialResult)
        {
            List<TPartialResult> partialResults = new List<TPartialResult>();
            foreach (TSource e in q1)
            {
                TQ2Result q2result = q2(e);                
                EmptyableResult<TPartialResult> partialResult = partialResultOf(e, q2result);
                if (!partialResult.isEmpty())
                    partialResults.Add(partialResult.Value);
            }


            return mergePartialResult(partialResults);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TQ2Result"></typeparam>
        /// <typeparam name="TPartialResult"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <param name="partialResultOf"></param>
        /// <param name="mergePartialResult"></param>
        /// <returns></returns>
        public static IEnumerable<TResult> NonAlgebraicLambdaOperatorUnionSeparable<TSource, TQ2Result,  TResult>(this IEnumerable<TSource> q1, Func<TSource, IEnumerable<TQ2Result>> q2, Func<TSource, TQ2Result, EmptyableResult<TResult>> partialResultOf)
        {
            
            foreach (TSource e in q1)
            {
                IEnumerable<TQ2Result> q2result = q2(e);
                foreach (TQ2Result q2r in q2result)
                {
                    EmptyableResult<TResult> partialResult = partialResultOf(e, q2r);
                    if (!partialResult.isEmpty())
                        yield return partialResult.Value;                   
                }
            }

            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TQ2Result"></typeparam>
        /// <typeparam name="TPartialResult"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <param name="partialResultOf"></param>
        /// <param name="mergePartialResult"></param>
        /// <returns></returns>
        public static IEnumerable<TResult> NonAlgebraicLambdaOperatorUnionSeparable<TSource, TQ2Result, TResult>(this IEnumerable<TSource> q1, Func<TSource, TQ2Result> q2, Func<TSource, TQ2Result, EmptyableResult<TResult>> partialResultOf)
        {    
            foreach (TSource e in q1)
            {
                TQ2Result q2result = q2(e);
                EmptyableResult<TResult> partialResult = partialResultOf(e, q2result);
                if (!partialResult.isEmpty())
                   yield return partialResult.Value;
            }
            
        }



        public class EmptyableResult<T>
        {
            internal static readonly EmptyableResult<T> emptyResult = new EmptyableResult<T>();
            protected EmptyableResult()
            {
            
            }
            internal EmptyableResult(T value)
            {
            
                this.value = value;
            }
            
            T value;
            internal bool isEmpty() { return this.Equals(emptyResult); }
            internal T Value { get { return value;  } }
            

        }

        //public class ResizableEnumerableCollection<T> : IEnumerable<T>
        //{
        //    List<T> elems = new List<T>();
        //    public void Add(T value)
        //    {
        //        elems.Add(value);
        //    }
            
        //}

        #endregion 
        public static IEnumerable<TResult> SBQLNavigate<TSource, TResult>(this IEnumerable<TSource> q1, Func<TSource, TResult> q2){
            return q1.NonAlgebraicLambdaOperatorUnionSeparable(q2, (e, q2r) => new EmptyableResult<TResult>(q2r));
        }

        public static IEnumerable<TResult> SBQLNavigate<TSource, TResult>(this IEnumerable<TSource> q1, Func<TSource, IEnumerable<TResult>> q2)
        {
            return q1.NonAlgebraicLambdaOperatorUnionSeparable(q2, (e, q2r) => new EmptyableResult<TResult>(q2r));
        }

        public static IEnumerable<TSource> SBQLWhere<TSource>(this IEnumerable<TSource> q1, Func<TSource, bool> q2)
        {
            return q1.NonAlgebraicLambdaOperatorUnionSeparable(q2, (e, q2r) => q2r ? new EmptyableResult<TSource>(e) : EmptyableResult<TSource>.emptyResult);

        }



        public static IEnumerable<TResult> SBQLJoin<TSource, TQ2Result, TResult>(this IEnumerable<TSource> q1, Func<TSource, TQ2Result> q2, Func<TSource, TQ2Result, TResult> joinResult)
        {
            return q1.NonAlgebraicLambdaOperatorUnionSeparable(q2, (e, q2r) => new EmptyableResult<TResult>(joinResult(e, q2r)));

        }

        public static IEnumerable<TResult> SBQLJoin<TSource, TQ2Result, TResult>(this IEnumerable<TSource> q1, Func<TSource, IEnumerable<TQ2Result>> q2, Func<TSource, TQ2Result, TResult> joinResult)
        {
            return q1.NonAlgebraicLambdaOperatorUnionSeparable(q2, (e, q2r) => new EmptyableResult<TResult>(joinResult(e, q2r)));

        }

        public static bool SBQLForAll<TSource>(this IEnumerable<TSource> q1, Func<TSource, bool> q2)
        {
            return q1.NonAlgebraicLambdaOperator(q2, (e, q2r) => new EmptyableResult<bool>(q2r), pr => { foreach (bool p in pr) { if (!p) return false; } return true; });
        }

        public static bool SBQLForAny<TSource>(this IEnumerable<TSource> q1, Func<TSource, bool> q2)
        {
            return q1.NonAlgebraicLambdaOperator(q2, (e, q2r) => new EmptyableResult<bool>(q2r), pr => { foreach (bool p in pr) { if (p) return true; } return false; });
        }

        //public static bool SBQLCloseBy<TSource>(this IEnumerable<TSource> q1, Func<TSource, TSource> q2)
        //{
        //    return q1.NonAlgebraicLambdaOperator(q2, (e, q2r) => new EmptyableResult<bool>(q2r), pr => { foreach (bool p in pr) { if (p) return true; } return false; });
        //}

        public static void test()
        {
            ICollection<Product> products = new List<Product>();
            Data.fillProducts(ref products);
            //select
           
            var result = products.SBQLNavigate(p => p.productName);

            //where
           
            products.SBQLWhere(p => p.unitPrice > 100);
            products.SBQLWhere(p => p.unitPrice > 100).SBQLNavigate(p => p.productName);
            //join
            var joinresult = products.SBQLJoin(p => p.productName, (p, q2r) => new { p, q2r });

          
 
            ////forall
            bool q1 =  products.SBQLForAll(p => p.unitPrice > 100);

            ////forany
            bool q2 = products.SBQLForAny(p => p.unitPrice > 100);
        }
    }
    
}
