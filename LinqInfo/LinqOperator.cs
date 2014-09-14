using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;


namespace LinqInfo
{
    using TypeHelpers;

    public static class LinqHelper
    {
        public static bool isLambdaExpression(this Expression e)
        {
            return e.NodeType == ExpressionType.Quote || e.NodeType == ExpressionType.Lambda;
        }

        public static LambdaExpression getLambdaExpression(Expression e)
        {
            LambdaExpression lambda = null;
            if (e.NodeType == ExpressionType.Quote)
                lambda = (e as UnaryExpression).Operand as LambdaExpression;
            else if (e.NodeType == ExpressionType.Lambda)
                lambda = e as LambdaExpression;
            return lambda;

        }

    }

    public class LINQOperator
    {
        public static readonly Type QUERYABLE = typeof(Queryable);
        public static readonly Type ENUMERABLE = typeof(Enumerable);

        public enum LINQOperatorType
        {
            Aggregate, Any, All, Average, Cast, Concat, Contains, Count, DefaultIfEmpty, Distinct, ElementAt, ElementAtOrDefault,
            Except, First, FirstOrDefault, GroupBy, GroupJoin, Intersect, Join, Last, LastOrDefault, LongCount, OrderBy, OrderByDescending, OfType,
            Reverse, ThenBy, ThenByDescending, Sum, SelectMany, Max, Min, Where, Select, SequenceEqual, Single, SingleOrDefault, Skip, SkipWhile, Take, TakeWhile,
            Union, Zip
        }

       

        static readonly Dictionary<MethodInfo, LINQOperator> method2operator = new Dictionary<MethodInfo, LINQOperator>();
        static readonly Dictionary<LINQOperatorType, List<LINQOperator>> type2operator = new Dictionary<LINQOperatorType, List<LINQOperator>>();
        static readonly Dictionary<string, LINQOperatorType> name2type = new Dictionary<string, LINQOperatorType>();

        public String Name { get { return Enum.GetName(typeof(LINQOperatorType), Type); } }
        public LINQOperatorType Type { get; internal set; }
        public Type DefinedBy { get { return Method.DeclaringType;} }
        public MethodInfo Method { get; internal set; }
        public bool IsLambda { get; set; }

        static LINQOperator()
        {
            foreach (LINQOperatorType t in Enum.GetValues(typeof(LINQOperatorType)))
            {
                name2type[Enum.GetName(typeof(LINQOperatorType), t)] = t;
            }


            var qoperators = typeof(Queryable).GetExtensionMethods(typeof(IQueryable<>)).Concat(typeof(Queryable).GetExtensionMethods(typeof(IOrderedQueryable<>))).Concat(typeof(Enumerable).GetExtensionMethods(typeof(IEnumerable<>))).Concat(typeof(Enumerable).GetExtensionMethods(typeof(IOrderedEnumerable<>)));            

            foreach (MethodInfo mi in qoperators)
            {

                if (!name2type.ContainsKey(mi.Name))
                    //        throw new InvalidOperationException("unknown linq operator: " + mi.Name);
                    continue;
                LINQOperatorType type = name2type[mi.Name];
                int paramsCount = mi.GetParameters().Count();
                int genericArgCount = mi.GetGenericArguments().Count();
                
                bool islambda = (mi.CountlambdaExpressionParameters() + mi.CountlambdaParameters()) > 0;

                LINQOperator oper = new LINQOperator() { Type = type, IsLambda = islambda, Method = mi };
                method2operator.Add(mi, oper);
                if (!type2operator.ContainsKey(type))
                {
                    type2operator.Add(type, new List<LINQOperator>());
                }
                type2operator[type].Add(oper);
            }
        }

        public static bool IsLinqOperatorCall(MethodInfo mi)
        {            
            if (mi.IsGenericMethod)
            {
                mi = mi.GetGenericMethodDefinition();
            }
            return method2operator.ContainsKey(mi);
        }

        public static LINQOperator GetLinqOperator(MethodInfo mi)
        {
            LINQOperator result = null;
            if (mi.IsGenericMethod && (!mi.IsGenericMethodDefinition))
            {
                mi = mi.GetGenericMethodDefinition();                
            }            
            result = GetOperator(mi);
            return result;
        }

        public static LINQOperator[] GetLinqOperator(LINQOperatorType type, Type definer, int paramCount, int genericArgCount)
        {            
            var results = type2operator[type].Where(o => o.Method.DeclaringType == definer && o.Method.GetGenericArguments().Count() == genericArgCount && o.Method.GetParameters().Count() == paramCount);
            if (results.Count() == 0)
            {
                throw new InvalidOperationException( "unable to find operator "  + type + " from '" + definer.Name + "' for params number: '" + paramCount + "' and generic arguments number: '" + genericArgCount + "'");
            }
            return results.ToArray();
        }

        static LINQOperator GetOperator(MethodInfo mi)
        {
            if (method2operator.ContainsKey(mi))
                return method2operator[mi];
            return null;
        }

    }
}
