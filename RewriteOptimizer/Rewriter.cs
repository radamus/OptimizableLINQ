using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;

namespace OptimizableLINQ
{
    using SBQL.Environment;
    using LinqInfo;

   
    public class Rewriter
    {
        static readonly MethodInfo asQueryable = typeof(Queryable).GetMethods().Where(m => m.Name == "AsQueryable" && m.IsGenericMethod).First();
        public Expression Optimize(Expression query)
        {

            Expression optimized = query;
            int i = 0;
            while (1 > i++)
            {
                query = optimized;
                var result = new BindingLevelCalculator().Extract(query);

                //foreach (string s in new BindingLevelsPrinter().Print(query, result))
                //{
                //    Console.WriteLine(s);
                //}
                //           Console.Write(result.ToString());

                IndependentSubquery q = new IndependencySearcher().Search(query, result);
                if (q != null)
                {

                    if (q.IndependentSubQuery.NodeType != ExpressionType.Constant && q.IndependentSubQuery.NodeType != ExpressionType.MemberAccess && q.IndependentSubQuery.NodeType != ExpressionType.Parameter)
                    {
                        optimized = new TreeModifier().Push(q.IndependentSubQuery, q.Query, query);

                    }
                }
            }// while (!optimized.Equals(query));
           


            return AsQueryableIfRequired(optimized);
        }

        private Expression AsQueryableIfRequired(Expression node)
        {
            if (typeof(IEnumerable<>).Name == node.Type.Name)
            {
                return Expression.Call(asQueryable.MakeGenericMethod(node.Type.GetGenericArguments()[0]), node);
            }
            else
                return node;
        }

    }
    
    
}
