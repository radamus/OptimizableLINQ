using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace OptimisableLINQ
{
    using LinqInfo;
    class OperatorParentCalculator : ExpressionVisitor
    {
        Stack<MethodCallExpression> opers = new Stack<MethodCallExpression>();
        List<MethodCallExpression> all = new List<MethodCallExpression>();
        Dictionary<Expression, MethodCallExpression> child2Parent = new Dictionary<Expression, MethodCallExpression>();

        internal Dictionary<Expression, MethodCallExpression> Child2Parent { get { return child2Parent; } }
        internal MethodCallExpression[] Operators { get { return all.ToArray(); } }

        internal void Calculate(Expression expr)
        {
            this.child2Parent = new Dictionary<Expression, MethodCallExpression>();
            Visit(expr);
            
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if(LINQOperator.IsLinqOperatorCall(node.Method))
            {
                Visit(node.Arguments[0]);
                all.Add(node);
                opers.Push(node);
                for (int i = 1; i < node.Arguments.Count; i++)
                {
                    Visit(node.Arguments[i]);
                }
             opers.Pop();
                return node;
            }
            return base.VisitMethodCall(node);
        }

        public override Expression Visit(Expression node)
        {
            if (opers.Count > 0)
            {
                child2Parent[node] = opers.Peek();
            }
            return base.Visit(node);
        }

    }
    class ParentCalculator :ExpressionVisitor
    {
        Dictionary<Expression, Expression> child2Parent = new Dictionary<Expression, Expression>();

        internal Dictionary<Expression, Expression> Calculate(Expression expr)
        {
            this.child2Parent = new Dictionary<Expression, Expression>();
            Visit(expr);
            return child2Parent;
        }
        protected override Expression VisitBinary(BinaryExpression node)
        {
            child2Parent[node.Left] = node;
            child2Parent[node.Right] = node;
            return base.VisitBinary(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            child2Parent[node.Operand] = node;
            return base.VisitUnary(node);
        }
          
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            foreach(ParameterExpression pe in node.Parameters)
                child2Parent[pe] = node;
            child2Parent[node.Body] = node;
            return base.VisitLambda<T>(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != null)
                child2Parent[node.Expression] = node;
            
            return base.VisitMember(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            foreach (Expression e in node.Arguments)
            child2Parent[e] = node;
            return base.VisitMethodCall(node);
        }
        protected override Expression VisitConditional(ConditionalExpression node)
        {
            child2Parent[node.Test] = node;
            child2Parent[node.IfTrue] = node;
            if (node.IfFalse != null)
            {
                child2Parent[node.IfFalse] = node;
            }
            return base.VisitConditional(node);
        }

        protected override Expression VisitBlock(BlockExpression node)
        {
            foreach (Expression e in node.Expressions)
                child2Parent[e] = node;
            return base.VisitBlock(node);
        }

        protected override Expression VisitLoop(LoopExpression node)
        {
            child2Parent[node.Body] = node;            
            return base.VisitLoop(node);
        }
        protected override Expression VisitNew(NewExpression node)
        {
            foreach (Expression e in node.Arguments)
            {
                child2Parent[e] = node;
            }
            return base.VisitNew(node);
        }
    }
}
