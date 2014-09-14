using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Text;

namespace LINQ_SBQLConverters
{
    using SBQL.Expressions;
    using SBQL.Environment;
    using TypeHelpers;
    using LinqInfo;

    public class SBQLLambdaExpression:SBQLExpression
    {
        public SBQLExpression Body { get; set; }
        internal LambdaInfo Lambda { get; set; }
        public new Type ResultType { get { return Lambda.ResultType; } }
        protected internal override object Accept(SBQLExpressionVisitor visitor)
        {
            throw new NotImplementedException();
        }

    }


    public class ParameterExpressionBinder :  Binder<ParameterExpression>
    {
        public static ParameterExpressionBinder CreateBinder(ParameterExpression pe)
        {
            return new ParameterExpressionBinder() { Expr = pe };
        }
        ParameterExpressionBinder() { }
        private ParameterExpression Expr { get; set;}
        public new String Name { get { return Expr.Name; } }
        public new ParameterExpression Value { get { return Expr; } }
    }
    public class ENVS4Linq : ENVS<ParameterExpression>
    {
        
        public  ParameterExpression BindParameter(String name)
        {
            BindingInfo<ParameterExpression> result = base.Bind(name);
            
            if (!result.Found)
                throw new WrongSBQLTree("name not found " + name);
            return result.Value;

        }

        
    }

   

    class LambdaInfo
    {
        public struct LambdaParameter
        {
            public Type Type { get; set; }
            public String Name { get; set; }
        }
        public List<LambdaParameter> parameters = new List<LambdaParameter>();
        public Type ResultType { get; set; }
    }

    

    public static class LINQOperators
    {
        
        public static readonly Dictionary<LINQOperator.LINQOperatorType, SBQLUnaryAlgebraicExpression.UnaryExpressionType> linqOperator2sbqlUnary = new Dictionary<LINQOperator.LINQOperatorType, SBQLUnaryAlgebraicExpression.UnaryExpressionType>();
        public static readonly Dictionary<LINQOperator.LINQOperatorType, SBQLBinaryAlgebraicExpression.BinaryExpressionType> linqOperator2sbqlBinary = new Dictionary<LINQOperator.LINQOperatorType, SBQLBinaryAlgebraicExpression.BinaryExpressionType>();
        public static readonly Dictionary<ExpressionType, SBQLBinaryAlgebraicExpression.BinaryExpressionType> linqExpressionType2sbqlBinary = new Dictionary<ExpressionType, SBQLBinaryAlgebraicExpression.BinaryExpressionType>();
        public static readonly Dictionary<SBQLBinaryAlgebraicExpression.BinaryExpressionType, ExpressionType> sbqlBinary2linqExpressionType = new Dictionary<SBQLBinaryAlgebraicExpression.BinaryExpressionType, ExpressionType>();
        public static readonly Dictionary<ExpressionType, SBQLUnaryAlgebraicExpression.UnaryExpressionType> linqExpressionType2sbqlUnary = new Dictionary<ExpressionType, SBQLUnaryAlgebraicExpression.UnaryExpressionType>();
        public static readonly Dictionary<SBQLUnaryAlgebraicExpression.UnaryExpressionType, ExpressionType> sbqlUnary2linqExpressionType = new Dictionary<SBQLUnaryAlgebraicExpression.UnaryExpressionType, ExpressionType>();
        
        
        static LINQOperators()
        {
            
            linqOperator2sbqlUnary.Add(LINQOperator.LINQOperatorType.Sum, SBQLUnaryAlgebraicExpression.UnaryExpressionType.Sum);
            linqOperator2sbqlUnary.Add(LINQOperator.LINQOperatorType.Average, SBQLUnaryAlgebraicExpression.UnaryExpressionType.Avg);
            linqOperator2sbqlUnary.Add(LINQOperator.LINQOperatorType.Max, SBQLUnaryAlgebraicExpression.UnaryExpressionType.Max);
            linqOperator2sbqlUnary.Add(LINQOperator.LINQOperatorType.Min, SBQLUnaryAlgebraicExpression.UnaryExpressionType.Min);
            linqOperator2sbqlUnary.Add(LINQOperator.LINQOperatorType.Any, SBQLUnaryAlgebraicExpression.UnaryExpressionType.Exists);
            linqOperator2sbqlUnary.Add(LINQOperator.LINQOperatorType.Count, SBQLUnaryAlgebraicExpression.UnaryExpressionType.Count);
           

            linqOperator2sbqlBinary.Add(LINQOperator.LINQOperatorType.Contains, SBQLBinaryAlgebraicExpression.BinaryExpressionType.Contains);

            linqExpressionType2sbqlUnary.Add(ExpressionType.Negate, SBQLUnaryAlgebraicExpression.UnaryExpressionType.Negate);

            foreach (ExpressionType key in linqExpressionType2sbqlUnary.Keys)
            {
                sbqlUnary2linqExpressionType.Add(linqExpressionType2sbqlUnary[key], key);
            }

            linqExpressionType2sbqlBinary.Add(ExpressionType.Add, SBQLBinaryAlgebraicExpression.BinaryExpressionType.Add);
            linqExpressionType2sbqlBinary.Add(ExpressionType.Subtract, SBQLBinaryAlgebraicExpression.BinaryExpressionType.Subtract);
            linqExpressionType2sbqlBinary.Add(ExpressionType.LessThan, SBQLBinaryAlgebraicExpression.BinaryExpressionType.LowerThan);
            linqExpressionType2sbqlBinary.Add(ExpressionType.LessThanOrEqual, SBQLBinaryAlgebraicExpression.BinaryExpressionType.LowerEqualThan);
            linqExpressionType2sbqlBinary.Add(ExpressionType.GreaterThan, SBQLBinaryAlgebraicExpression.BinaryExpressionType.GreaterThan);
            linqExpressionType2sbqlBinary.Add(ExpressionType.GreaterThanOrEqual, SBQLBinaryAlgebraicExpression.BinaryExpressionType.GreaterEqualThan);
            linqExpressionType2sbqlBinary.Add(ExpressionType.Divide, SBQLBinaryAlgebraicExpression.BinaryExpressionType.Divide);
            linqExpressionType2sbqlBinary.Add(ExpressionType.Multiply, SBQLBinaryAlgebraicExpression.BinaryExpressionType.Multiply);
            linqExpressionType2sbqlBinary.Add(ExpressionType.Equal, SBQLBinaryAlgebraicExpression.BinaryExpressionType.Equal);
            linqExpressionType2sbqlBinary.Add(ExpressionType.NotEqual, SBQLBinaryAlgebraicExpression.BinaryExpressionType.NotEqual);

            foreach(ExpressionType key in linqExpressionType2sbqlBinary.Keys)
            {
                sbqlBinary2linqExpressionType.Add(linqExpressionType2sbqlBinary[key], key);
            }

            

        }

       
    }

    public class SBQLLinqExpressionWrapper : SBQLWrapperExpression<Expression>
    {
             
    }
}
