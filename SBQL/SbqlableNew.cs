using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBQL
{/*
    class M<T> : IEnumerable<T>
    {
    }
    static class SbqlableNew
    {

       


        IEnumerable<TR> Eval<TS, TQ2R, TPartR, TR>(IEnumerable<TS> source, Func<TS, IEnumerable<TQ2R>> evalQ2,
            Func<TS, IEnumerable<TQ2R>, IEnumerable<TPartR>> partialResultOf, Func<IEnumerable<TPartR>, IEnumerable<TR>> mergePartialResult)
        {

            List<TPartR> partialResults = new List<TPartR>();
            foreach (TS e in source)
            {
                IEnumerable<TQ2R> res = evalQ2(e);
                partialResults.AddRange(partialResultOf(e, res));
            }

            return mergePartialResult(partialResults);
        }

        IEnumerable<TR> EvalLazy<TS, TQ2R,TR>(IEnumerable<TS> source, Func<TS, IEnumerable<TQ2R>> evalQ2,
            Func<TS, IEnumerable<TQ2R>, IEnumerable<TR>> partialResultOf)
        {

            foreach (TS e in source)
            {
                IEnumerable<TQ2R> res = evalQ2(e);
                foreach (TR pres in partialResultOf(e, res))
                    yield return pres;
            }
        }

        IEnumerable<TR> Bind<TS,  TR>(IEnumerable<TS> source, Func<TS, IEnumerable<TR>> evalQ2)
        {
            foreach (TS e in source)
            {                
                foreach (TR pres in evalQ2(e))
                    yield return pres;
            }
        }


      
    }*/
}
