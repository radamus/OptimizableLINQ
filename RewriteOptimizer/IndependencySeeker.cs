using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Optimizer
{
    using LinqInfo;

    public class IndependentSubquery
    {
        public Expression Query { get; internal set; }
        public Expression IndependentSubQuery { get; internal set; }

    }

    /// <summary>
    /// Search for an operator in the tree and look for the operator subtree that is independent from the operator    
    /// </summary>
    internal class IndependencySearcher : ExpressionVisitor
    {

        class IndependentSubQuerySearcher : ExpressionVisitor
        {
            internal BindingLevelsInfo BindingInfo { get; private set; }
            private Stack<BindingLevelsInfo.Operator> context = new Stack<BindingLevelsInfo.Operator>();
            internal Expression independentSubQuery;
            IndependencyChecker checker = new IndependencyChecker();
            internal Expression FindIndependentSubQuery(BindingLevelsInfo bindingInfo, BindingLevelsInfo.Operator context, Expression expr)
            {
                this.BindingInfo = bindingInfo;
                this.context.Clear();
                this.context.Push(context);
                Visit(expr);
                return independentSubQuery;
            }

            public override Expression Visit(Expression node)
            {
                if (node == null)
                    return node;
                if (checker.IsIndependent(BindingInfo, context.ToArray(), node))
                {
                    this.independentSubQuery = node;
                    return node;
                }
                else
                    return base.Visit(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {

                if (BindingInfo.isOperator(node))
                {
                    Visit(node.Arguments[0]);
                    this.context.Push(BindingInfo.GetOperator(node));
                    for (int i = 1; i < node.Arguments.Count; i++)
                    {
                        Visit(node.Arguments[i]);
                    }
                    this.context.Pop();
                    return node;

                }else
                return base.VisitMethodCall(node);
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                Visit(node.Body);
                return node;
            }
            //protected override Expression VisitMethodCall(MethodCallExpression node)
            //{
            //    if (checker.IsIndependent(BindingInfo, Context, node))
            //        this.independentSubQuery = node;
            //    else
            //    {
            //        Expression arg = node.Object == null ? node.Arguments[0] : node.Object;
            //        Visit(arg);
            //    }
            //    return node;
            //}
        }
        /// <summary>
        /// Checks if the whole argument query is independent from context operators
        /// </summary>
        class IndependencyChecker : ExpressionVisitor
        {
            internal BindingLevelsInfo BindingInfo { get; private set; }
            internal BindingLevelsInfo.Operator[] Context { get; private set; }
            private bool isIndependent = true;
            private bool bindAnyName = false;

            internal bool IsIndependent(BindingLevelsInfo bindingInfo, BindingLevelsInfo.Operator[] context, Expression expr)
            {
                BindingInfo = bindingInfo;
                Context = context;
                isIndependent = true;
                bindAnyName = false;
                Visit(expr);
                return isIndependent && bindAnyName;
            }
            public override Expression Visit(Expression node)
            {
                if (!isIndependent)
                    return node;
                return base.Visit(node);
            }
            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                Visit(node.Body);
                return node;
            }
            protected override Expression VisitParameter(ParameterExpression node)
            {
                bindAnyName = true;
                int bind = BindingInfo.getBindinglevelForBindExpression(node);
                if(Context.Any(c => c.OpenedSectionNumber == bind))
                    isIndependent = false;                
                return node;
            }
        }

        internal BindingLevelsInfo BindingInfo { get; private set; }


        IndependentSubquery independent = null;

        public IndependentSubquery Search(Expression input, BindingLevelsInfo bindingInfo)
        {
            this.BindingInfo = bindingInfo;
            Visit(input);
            return independent;
        }


        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (BindingInfo.isOperator(node))
            {

                BindingLevelsInfo.Operator oper = BindingInfo.GetOperator(node);

                foreach (Expression e in node.Arguments)
                {
                    if (e.NodeType == ExpressionType.Quote || e.NodeType == ExpressionType.Lambda)
                    {
                        Expression body = LinqHelper.getLambdaExpression(e).Body;
                        
                        Expression subQuery = new IndependentSubQuerySearcher().FindIndependentSubQuery(BindingInfo, oper, body);
                        if (subQuery != null)
                        {
                            this.independent = new IndependentSubquery() { Query = oper.OperatorExpr, IndependentSubQuery = subQuery };
                        }
                        else
                        {
                             IndependentSubquery isubQuery = new IndependencySearcher().Search(body, BindingInfo);
                             if (isubQuery != null)
                             {
                                 this.independent = isubQuery;
                             }
                        }
                        
                    }
                    else Visit(e);
                }
                return node;
            }
            else
                return base.VisitMethodCall(node);
        }


    }

}
