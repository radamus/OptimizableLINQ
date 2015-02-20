using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptimizableLINQ
{
    class OptimizationException : Exception
    {
        public OptimizationException(string message)
            : base(message)
        {
        }
    }
}
