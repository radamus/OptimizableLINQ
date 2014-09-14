using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LINQ_SBQLConverters
{
    class WrongSBQLTree : Exception
    {
        

        public WrongSBQLTree(string p):base(p)
        {
            
        }
    }
}
