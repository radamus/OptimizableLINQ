using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace OptimizableLINQ
{
    using TypeHelpers;
    internal class TreeModifier : ExpressionVisitor
    {
        static int aux_idx = 0;
        Dictionary<Expression, Expression> pushed = new Dictionary<Expression, Expression>();
        internal Expression Push(Expression pushable, Expression pushBefore, Expression query)
        {

            Expression pushed = this.CreatePushedExpression(pushable, pushBefore);
            query = new TreeNodeReplacer().Replace(query, pushBefore, pushed);
            return query;
        }

        


        MethodCallExpression CreatePushedExpression(Expression pushable, Expression pushedBefore)
        {
            TreeNodeReplacer replacer = new TreeNodeReplacer();
            List<Expression> separated = Separate(pushable);
            Stack<Expression> parts = new Stack<Expression>();
            Stack<ParameterExpression> auxs = new Stack<ParameterExpression>();            
            Expression replaced = pushable;
            foreach(Expression s in separated){
                
                parts.Push(s);
                auxs.Push(Expression.Parameter(typeof(IGrouping<,>).MakeGenericType(typeof(int), parts.Peek().Type.GetGenericArguments()[0]), makeAuxName()));
               
                replaced = replacer.Replace(replaced, parts.Peek(), auxs.Peek());
            }

            Expression subexpr = generateSelect(generateGroupBy(parts.Pop(), auxs.Peek().Type.GetGenericArguments()[1]),replaced,auxs.Pop());

            ParameterExpression aux = Expression.Parameter(subexpr.Type.GetGenericArguments()[0],makeAuxName());
            foreach(Expression part in parts)
            {
                subexpr = generateSelectMany(generateGroupBy(part, auxs.Peek().Type.GetGenericArguments()[1]), subexpr, auxs.Pop());
            }

            return generateSelectMany(subexpr, replacer.Replace(pushedBefore, pushable, aux), aux);

        }

        private string makeAuxName()
        {
            return ("_<aux" + checked(aux_idx++) + ">_");            
        }

        private Expression FindFirstEnumerableResultSubQuery(Expression pushable)
        {
            Expression result = new ExpressionFinder().FindFirst(pushable, p => (p.Type.IsQueryTypeResult() 
                                                                                && p.NodeType != ExpressionType.Constant &&
                                                                                p.NodeType != ExpressionType.MemberAccess)
                                                                );
         //   if (result == null)
          //      throw new OptimizatorFatalException("unable to find enumerable result" + pushable);
            return result;
        }

        private List<Expression> Separate(Expression pushable)
        {
            List<Expression> result = new List<Expression>();
            if (pushable as BinaryExpression != null)
            {
                BinaryExpression binExpr = (BinaryExpression)pushable;
                result.AddRange(Separate(binExpr.Left));
                result.AddRange(Separate(binExpr.Right));
            }
            else
            {
                Expression res = FindFirstEnumerableResultSubQuery(pushable);
                if(res != null)
                    result.Add(res);
            }

            return result;
        }

        private MethodCallExpression generateGroupBy(Expression query, Type sourceType)
        {            
            Type keyType = typeof(int);       
     
            MethodInfo groupSpecificMethod = getOperatorMethod(query.Type, LinqInfo.LINQOperator.LINQOperatorType.GroupBy, 2, sourceType, keyType);
            ParameterExpression lambdaParam = Expression.Parameter(sourceType, "_<key>_");
            ConstantExpression body = Expression.Constant(0);
            LambdaExpression lambda = generateLambda(groupSpecificMethod.GetParameters()[1].ParameterType, body, lambdaParam);
            return Expression.Call(groupSpecificMethod, query, makeLambdaMethodArgument(groupSpecificMethod, lambda));

        }

        private LambdaExpression generateLambda(Type type, Expression body, ParameterExpression lambdaParam)
        {
            if(typeof(Expression<>).Name == type.Name)
                type = type.GetGenericArguments()[0];
            if (!type.IsLambdaType())
                throw new OptimizationFatalException("Expression<> argument requires delegate type");

            return Expression.Lambda(type, body, lambdaParam);
        }

        private Expression generateSelect(Expression query, Expression body, ParameterExpression lambdaParam)
        {   
            if(body.Equals(lambdaParam))
            {
                return query;
            }
            MethodInfo selectSpecificMethod = getOperatorMethod(query.Type, LinqInfo.LINQOperator.LINQOperatorType.Select, 2, lambdaParam.Type, body.Type);

            LambdaExpression lambda = generateLambda(selectSpecificMethod.GetParameters()[1].ParameterType, body, lambdaParam);
            return Expression.Call(selectSpecificMethod, query, makeLambdaMethodArgument(selectSpecificMethod, lambda));

        }

        private MethodCallExpression generateSelectMany(Expression query, Expression body, ParameterExpression lambdaParam)
        {
            Type bodyType = body.Type;
            if (!body.Type.IsQueryTypeResult())
            {
                throw new OptimizationFatalException("unable to create SelectMany with lambda body expression of type " + body.Type);
            }
            MethodInfo groupSpecificMethod = getOperatorMethod(query.Type, LinqInfo.LINQOperator.LINQOperatorType.SelectMany, 2, lambdaParam.Type, body.Type.GetGenericArguments()[0]);

            
            LambdaExpression lambda = generateLambda(groupSpecificMethod.GetParameters()[1].ParameterType, body, lambdaParam);     
            return Expression.Call(groupSpecificMethod, query, makeLambdaMethodArgument(groupSpecificMethod, lambda));

        }

      

        private MethodInfo getOperatorMethod(Type thisType, LinqInfo.LINQOperator.LINQOperatorType lINQOperatorType, int paramNumber, params Type[] genericArguments)
        {
            LinqInfo.LINQOperator oper;
            Type etype;
            if (thisType.Name == typeof(IQueryable<>).Name)
                etype = typeof(Queryable);
            else
                etype = typeof(Enumerable);

            oper = LinqInfo.LINQOperator.GetLinqOperator(lINQOperatorType, etype, paramNumber, genericArguments.Count())[0];
            MethodInfo genericMethod = oper.Method;
            MethodInfo specificMethod = genericMethod.MakeGenericMethod(genericArguments);
            return specificMethod;
        }


        private Expression makeLambdaMethodArgument(MethodInfo operatorMethod, LambdaExpression lambda)
        {
            Expression lambdaArgument = lambda;
            string ename = typeof(Expression<>).Name;
            if (operatorMethod.GetParameters().Where(p => p.ParameterType.Name == ename).Any())
            {
                lambdaArgument = Expression.MakeUnary(ExpressionType.Quote, lambda, lambda.Type);
            }

            return lambdaArgument;
        }
        
    }

    class ExpressionFinder : ExpressionVisitor
    {
        Func<Expression, bool> comparator;
        Expression found;

        internal Expression FindFirst(Expression root, Func<Expression, bool> comparator)
        {
            this.comparator = comparator;
            Visit(root);
            return found;
        }
        
        public override Expression Visit(Expression node)
        {
            if (node == null)
                return node;
            if (comparator(node))
            {
                found = node;
                return node;
            }
            return base.Visit(node);
        }
    }
}
