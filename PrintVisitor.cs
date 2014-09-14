using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Visitors
{
    
    public class PrintVisitor : ExpressionVisitor
    {
        StringBuilder builder;
        
        public string eval(Expression exp)
        {
            builder = new StringBuilder();
            Visit(exp);
            return builder.ToString();
        }
        protected override Expression VisitUnary(UnaryExpression node)
        {
            builder.AppendLine("Unary node type " + node.NodeType);
            if (node.Method != null)
                builder.AppendLine("Unary method " + node.Method.Name.ToString());
            builder.AppendLine("Unary operand " + node.Operand.ToString() + ", type: " + node.Operand.Type + ", node type: " + node.Operand.NodeType);
            
            return base.VisitUnary(node);
        }
        protected override Expression VisitBinary(BinaryExpression exp)
        {
            builder.AppendLine("Binary node type " + exp.NodeType );
            builder.AppendLine("Binary type " + exp.Type);
            builder.AppendLine("Binary left " + exp.Left.ToString() + ", type: " + exp.Left.Type +", node type: " + exp.Left.NodeType);

            builder.AppendLine("Binary right " + exp.Right.ToString() + ", type: " + exp.Right.Type +", node type: " + exp.Right.NodeType);

            if (exp.Method != null)
               builder.AppendLine("Binary method " + exp.Method.Name.ToString());


            return base.VisitBinary(exp);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            builder.AppendLine("Call " + node.Method.Name.ToString());
            builder.AppendLine("Call return type: " + node.Type );

            foreach (Expression e in node.Arguments)
            {
                builder.AppendLine("Argument: type " + e.Type.Name + " node type " + e.NodeType); 
            }
            return base.VisitMethodCall(node);
        }
        
        protected override Expression VisitConstant(ConstantExpression node)
        {
            builder.AppendLine(node.NodeType.ToString());
            builder.AppendLine("Value: " + node.Value + " type " + node.Type );
            return base.VisitConstant(node);
        }
        protected override Expression VisitParameter(ParameterExpression node)
        {

            builder.AppendLine("Parameter name " + node.Name.ToString() + ", type: " + node.Type );

            
            return base.VisitParameter(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            builder.AppendLine("Lambda, return type: " + node.ReturnType);
            foreach(ParameterExpression pe in node.Parameters){
                builder.AppendLine("Parameter " + pe.Name);
            }
            return base.VisitLambda<T>(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            builder.AppendLine("Member " + node.Member.ToString());
     //       printNodeType(builder, node);
            
            builder.AppendLine("Member expression: ");// + node.Expression.ToString() + ", node type: " + node.Expression.NodeType);  
          
            return base.VisitMember(node);
        }

        private void printNodeType(StringBuilder builder, Expression node)
        {
            builder.AppendLine("Node " + node.NodeType + ", static type " + node.Type);
        }

        
        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {
            builder.AppendLine("Member binding " + node.Member.ToString());
            builder.AppendLine("Member binding type " + node.Member.ToString());
            return base.VisitMemberBinding(node);
        }

        protected override Expression VisitDynamic(DynamicExpression node)
        {

            return base.VisitDynamic(node);
        }
        public override Expression Visit(Expression node)
        {


            return base.Visit(node);
        }
    }

}
