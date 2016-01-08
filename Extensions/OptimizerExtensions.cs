using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizableLINQ
{
    public class MyLazy<TSource>
    {
        private TSource value;
        private bool materialised = false;
        private Func<TSource> func;

        public TSource Value
        {

            get 
            {
                if (!materialised)
                {
                    materialised = true;
                    value = func();
                }
                    
                return value;
            }
        }

        public MyLazy(Func<TSource> func) {
            this.func = func;
        }
    }

    public static class OptimizerExtensions
    {
        public static int Count(this IEnumerable source)
        {
            int res = 0;
            foreach (var item in source)
                res++;
            return res;
        }

        // groupas solution - for performance
        public static IEnumerable<TResult> GroupSelect<TSource, TResult>(this IEnumerable<TSource> source, Func<IEnumerable<TSource>, IEnumerable<TResult>> selector)
        {
            foreach (var result in selector(source.ToList()))
                yield return result;
        }

        // groupas solution - for evaluation with possible errors
        public static IEnumerable<TResult> DelayedGroupSelect<TSource, TResult>(this IEnumerable<TSource> source, Func<Lazy<IEnumerable<TSource>>, IEnumerable<TResult>> selector)
        {
            foreach (var result in selector(new Lazy<IEnumerable<TSource>>(() => source.ToList())))
                yield return result;
        }

        //The best...
        public static IEnumerable<TSource> AsGroup<TSource>(this Func<TSource> sourceFunc)
        {
            yield return sourceFunc();
        }

        //The best...
        public static IEnumerable<Lazy<TSource>> AsGroupSuspendedThreadSafe<TSource>(this Func<TSource> sourceFunc)
        {
            yield return new Lazy<TSource>(sourceFunc);
        }

/*        public static IEnumerable<Lazy<TSource>> AsGroupSuspended<TSource>(this Func<TSource> sourceFunc)
        {
            yield return new Lazy<TSource>(sourceFunc, false);
        }
*/
        public static IEnumerable<MyLazy<TSource>> AsGroupSuspended<TSource>(this Func<TSource> sourceFunc)
        {
            yield return new MyLazy<TSource>(sourceFunc);
        }

        public static IEnumerable<MyLazy<IEnumerable<TSource>>> AsGroupSuspended<TSource>(this IEnumerable<TSource> source)
        {
            Func<IEnumerable<TSource>> sourceFunc = () => source.ToList();
            yield return new MyLazy<IEnumerable<TSource>>(sourceFunc);
        }

        public static IEnumerable<IEnumerable<TSource>> Group<TSource>(this IEnumerable<TSource> source)
        {
            yield return source.ToList();
        }

        public static VolatileIndex<TKey, TSource> ToVolatileIndex<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return VolatileIndex<TKey, TSource>.Create(source, keySelector);
        }

        public static AlmostVolatileIndex<TKey, TSource> ToAlmostVolatileIndex<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return AlmostVolatileIndex<TKey, TSource>.Create(source, keySelector, s => s, null);
        }
    }
}
