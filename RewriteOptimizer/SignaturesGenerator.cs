using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Optimizer
{
    using LinqInfo;
    public class Signature
    {
        public enum SignatureKind {Reference, Value, Binder}

        public SignatureKind Kind { get; private set; }
        public Type Type { get; private set; }
        public Cardinality Cardinality { get; private set; }

        public static Signature ValueSignature(Type type, Cardinality cardinality)
        {
            return new Signature() { Kind = SignatureKind.Value, Type = type, Cardinality = cardinality };
        }

        public static Signature ReferenceSignature(Type type, Cardinality cardinality)
        {
            return new Signature() { Kind = SignatureKind.Reference, Type = type, Cardinality = cardinality };
        }

        public static Signature BinderSignature(String name, Type type, Cardinality cardinality)
        {
            return new BinderSignature() { Kind = SignatureKind.Binder, Type = type, Cardinality = cardinality , Name = name};
        }
    }

    public class BinderSignature : Signature
    {
        public String Name { get; internal set; }
    }

    public enum Cardinality
    {
        ZeroOrMore, Single
    }

    class TreeSignatures
    {
        Dictionary<Expression, Signature> expr2Sig = new Dictionary<Expression,Signature>();

        public void Add(Expression e, Signature s)
        {
            if (expr2Sig.ContainsKey(e) && expr2Sig[e] != s)
                throw new OptimizationFatalException("node already has a signature " + e + " use 'Modify'");
            expr2Sig[e] = s;
        }

        public void Modify(Expression e, Signature s)
        {
            if (!expr2Sig.ContainsKey(e))
                throw new OptimizationFatalException("node does not have a signature " + e);
            expr2Sig[e] = s;
        }

        public Signature Get(Expression e)
        {
            if (!expr2Sig.ContainsKey(e))
            {
                throw new OptimizationFatalException("node does not have a signature " + e);
            }
            return expr2Sig[e];
        }
    }
    class SignaturesGenerator : ExpressionVisitor
    {
        TreeSignatures treeSignatures;
        TreeSignatures Generate(Expression e)
        {
            treeSignatures = new TreeSignatures();
            Visit(e);
            return treeSignatures;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (LINQOperator.IsLinqOperatorCall(node.Method))
            {
                LINQOperator oper = LINQOperator.GetLinqOperator(node.Method);
                Visit(node.Arguments[0]);
                Signature lsig = this.treeSignatures.Get(node.Arguments[0]);

                //TODO
                if (lsig.Cardinality == Cardinality.Single && oper.Type == LINQOperator.LINQOperatorType.Where)
                {
                }
                //if groupby lambda body == constant => operator cardinality = 1

            }
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            this.treeSignatures.Add(node, Signature.BinderSignature(node.Name, node.Type, CalculateCardinality(node.Type)));
            return node;
        }
        protected override Expression VisitUnary(UnaryExpression node)
        {
            Visit(node.Operand);
            this.treeSignatures.Add(node, Signature.ReferenceSignature(node.Type, CalculateCardinality(node.Type)));
            return node;            
        }
        protected override Expression VisitBinary(BinaryExpression node)
        {
            Visit(node.Left);
            Visit(node.Right);
            this.treeSignatures.Add(node, Signature.ReferenceSignature(node.Type, CalculateCardinality(node.Type)));
            return node;
        }

        /// <summary>
        /// node.Expression == null => static field
        /// node.Expression.NodeType == ExpressionType.Constant => local variable
        /// node.Expression.NodeType == ExpressionType.Call => enumerable as lambda param, 
        /// node.Expression.NodeType == ExpressionType.MemberAccess => path
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            //static field
            if (node.Expression != null)
            {
                Visit(node.Expression);
                
            }
            this.treeSignatures.Add(node, Signature.ReferenceSignature(node.Type, CalculateCardinality(node.Type)));            

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Type.IsPrimitive || node.Value as String != null)
            {
                this.treeSignatures.Add(node, Signature.ValueSignature(node.Type, Cardinality.Single));
            }
            else
            {
                Cardinality card = CalculateCardinality(node.Type);
                this.treeSignatures.Add(node, Signature.ReferenceSignature(node.Type, card));
            }
            return node;
        }

        private Cardinality CalculateCardinality(Type type)
        {
            String enumerableName = typeof(IEnumerable<>).Name;
            if (enumerableName == type.Name || (type.GetInterface(enumerableName) != null))
                return Cardinality.ZeroOrMore;
            return Cardinality.Single;
        }

        private Cardinality CalculateContextCardinality(Cardinality context, Type type)
        {
            String enumerableName = typeof(IEnumerable<>).Name;
            if (enumerableName == type.Name || (type.GetInterface(enumerableName) != null))
                return Cardinality.ZeroOrMore;
            return Cardinality.Single;
        }


        
    }
}
