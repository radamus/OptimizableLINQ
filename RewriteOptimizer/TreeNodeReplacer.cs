using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Diagnostics;

namespace Optimizer
{
    using TypeHelpers;
    class TreeNodeReplacer
    {


        public Expression Replace(Expression expr, Expression oldNode, Expression newNode)
        {

            if (expr == oldNode)
            {
                return newNode;
            }

            Expression result = null;
            Expression prev = oldNode;
            Expression replacer = newNode;


            Dictionary<Expression, Expression> child2parent = new ParentCalculator().Calculate(expr);
            List<Expression> childParentChain = new List<Expression>();
            Expression child = prev;
            while (child2parent.ContainsKey(child))
            {
                Expression parent = child2parent[child];
                childParentChain.Add(parent);
                child = parent;
            }

            child = prev;
            foreach (Expression parent in childParentChain)
            {
                result = CreateParentWithReplacedChild(parent, child, replacer);
                child = parent;
                replacer = result;
            }
            if (result == null)
            {
                throw new OptimizationFatalException("unable to replace " + oldNode + " with " + newNode + " in " + expr);
            }
            return result;

        }


        Expression CreateParentWithReplacedChild(Expression parent, Expression oldChild, Expression newChild)
        {
            Expression result;
            if (parent is BinaryExpression)
            {
                result = replaceBinaryNodeChild(parent as BinaryExpression, oldChild, newChild);
            }
            else if (parent is UnaryExpression)
            {
                result = replaceUnaryNodeChild(parent as UnaryExpression, oldChild, newChild);
            }
            else if (parent is MethodCallExpression)
            {
                result = replaceMethodCallNodeChild(parent as MethodCallExpression, oldChild, newChild);
            }
            else if (parent is LambdaExpression)
            {
                result = replaceLambdaNodeChild(parent as LambdaExpression, oldChild, newChild);
            }
            else if (parent is ConditionalExpression)
            {
                result = replaceConditionalNodeChild(parent as ConditionalExpression, oldChild, newChild);
            }
            
            else
            {
                throw new OptimizationFatalException("creating expression of type " + parent.NodeType + " is currently unsupported");
            }

            return result;
        }

     

        private ConditionalExpression replaceConditionalNodeChild(ConditionalExpression conditionalExpression, Expression oldChild, Expression newChild)
        {
            Expression test = conditionalExpression.Test.Equals(oldChild) ? newChild : conditionalExpression.Test;
            Expression ifTrue = conditionalExpression.IfTrue.Equals(oldChild) ? newChild : conditionalExpression.IfTrue;
            Expression ifFalse = conditionalExpression.IfFalse.Equals(oldChild) ? newChild : conditionalExpression.IfFalse;
            return conditionalExpression.Update(test, ifTrue, ifFalse);            
            
        }

        private LambdaExpression replaceLambdaNodeChild(LambdaExpression node, Expression oldChild, Expression newChild)
        {
            if (!node.Body.Equals(oldChild))
                throw new OptimizationFatalException("unable to replace lambda parameter" + oldChild);           
            return Expression.Lambda(newChild, node.Parameters);
        }

        private UnaryExpression replaceUnaryNodeChild(UnaryExpression node, Expression oldChild, Expression newChild)
        {
            return node.Update(newChild);            
        }

        private MethodCallExpression replaceMethodCallNodeChild(MethodCallExpression node, Expression oldChild, Expression newChild)
        {
            List<Expression> arguments = new List<Expression>();
            List<Type> typearguments = new List<Type>();
            MethodInfo method = node.Method;

            foreach (Expression arg in node.Arguments)
            {
                arguments.Add(arg.Equals(oldChild) ? newChild : arg);
            }
            MethodCallExpression result;
            try
            {
                result = Expression.Call(method, fixArguments(method, arguments));
            }
            catch (ArgumentException e)
            {
                LinqInfo.LINQOperator oper;
                Type definer;
                if (LinqInfo.LINQOperator.IsLinqOperatorCall(node.Method))
                {
                    oper = LinqInfo.LINQOperator.GetLinqOperator(node.Method);
                    definer = oper.DefinedBy == LinqInfo.LINQOperator.QUERYABLE ? LinqInfo.LINQOperator.ENUMERABLE : LinqInfo.LINQOperator.QUERYABLE;
                    
                    oper = LinqInfo.LINQOperator.GetLinqOperator(oper.Type, definer, node.Arguments.Count, oper.Method.GetGenericArguments().Count())[0];
                    
                }              
                else
                    throw new OptimizationFatalException("the method " + node.Method + "is incompatible with replaced node " + newChild, e);

                method = oper.Method;
                
                method = method.MakeGenericMethod(node.Method.GetGenericArguments());
                result = Expression.Call(method, fixArguments(method, arguments));


            }

            return result;
        }

        /// <summary>
        /// Performe changes on arguments expression according to method info
        /// </summary>
        /// <param name="method"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private IEnumerable<Expression> fixArguments(MethodInfo method, List<Expression> arguments)
        {
            Func<Expression, Expression> selector = a => a;
            if (method.DeclaringType.Equals(typeof(Enumerable)))
                selector = a => (a.NodeType == ExpressionType.Quote) ? (a as UnaryExpression).Operand : a;
            else if (method.DeclaringType.Equals(typeof(Queryable)))
                selector = a => (a.NodeType == ExpressionType.Lambda) ? Expression.MakeUnary(ExpressionType.Quote, a, a.Type) : a;
            return arguments.Select(a => selector(a));
        }



        private BinaryExpression replaceBinaryNodeChild(BinaryExpression node, Expression oldChild, Expression newChild)
        {
            Expression leftOperand = node.Left.Equals(oldChild) ? newChild : node.Left;
            Expression rightOperand = node.Right.Equals(oldChild) ? newChild : node.Right;
            return node.Update(leftOperand, node.Conversion, rightOperand);            
        }


    }
}
