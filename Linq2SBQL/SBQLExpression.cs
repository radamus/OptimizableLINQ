using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace SBQL.Expressions
{

    public abstract class SBQLExpression
    {
        enum NodeType
        {
            Sum, Avg, Max, Min, Negate,
            Exists,  Count, GreaterThan, GreaterEqualThan, LowerThan, LowerEqualThan, Equal, NotEqual, Add, Subtract, Multiply, Divide,  Intersect, Minus,
            Contains, Where, Dot, Join, ForAll, ForSome
        }
        public SBQLExpression Parent { get; set; }
        public Type ResultType { get; set; }

        

        internal protected abstract object Accept(SBQLExpressionVisitor visitor);

        public static SBQLNonAlgebraicExpression NonAlgebraicExpression(SBQLNonAlgebraicExpression.NonAlgebraicOperator operatorType, Type resultType)
        {
            return new SBQLNonAlgebraicExpression { OperatorType = operatorType, ResultType = resultType};
        }

        public static SBQLBinaryAlgebraicExpression BinaryAlgebraicExpression(SBQLBinaryAlgebraicExpression.BinaryExpressionType operatorType, Type resultType)
        {
            return new SBQLBinaryAlgebraicExpression { OperatorType = operatorType, ResultType = resultType };
        }

        public static SBQLUnaryAlgebraicExpression UnaryAlgebraicExpression(SBQLUnaryAlgebraicExpression.UnaryExpressionType operatorType, Type resultType)
        {
            return new SBQLUnaryAlgebraicExpression { OperatorType = operatorType, ResultType = resultType };
        }

        internal static SBQLAuxNameExpression AuxNameExpression(String auxName, Boolean group, Type resultType)
        {
            return new SBQLAuxNameExpression { Name = auxName, Group = group, ResultType = resultType };
        }

        internal static SBQLNameExpression NameExpression(string name, Type resultType)
        {
            return new SBQLNameExpression { Name = name, ResultType = resultType };

        }

       

        internal static SBQLProcedureCallExpression ProcedureCallExpression()
        {
            return new SBQLProcedureCallExpression {  };

        }

        internal static SBQLExpression LiteralExpression(Type type, object p)
        {
            if (type.Equals(typeof(int)))
            {
                return new SBQLIntLiteral { Value = (int)p };
            }
            else if (type.Equals(typeof(string)))
            {
                return new SBQLStringLiteral { Value = (string)p };
            }
            else if (type.Equals(typeof(double)))
            {
                return new SBQLDoubleLiteral { Value = (double)p };
            }
            else if (type.Equals(typeof(bool)))
            {
                return new SBQLBooleanLiteral { Value = (bool)p };
            }
            throw new NotImplementedException("literal type " + type.Name);
        }
    }

    public abstract class SBQLLiteralExpression<T> : SBQLExpression
    {
        public T Value { get; set; }
        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class SBQLStringLiteral : SBQLLiteralExpression<string>
    {
        protected internal override object Accept(SBQLExpressionVisitor visitor)
        {
            return visitor.VisitLiteralExpression<string>(this);
        }
    }

    public class SBQLIntLiteral : SBQLLiteralExpression<int>
    {
        protected internal override object Accept(SBQLExpressionVisitor visitor)
        {
            return visitor.VisitLiteralExpression<int>(this);
        }
    }

    public class SBQLDoubleLiteral : SBQLLiteralExpression<double>
    {
        protected internal override object Accept(SBQLExpressionVisitor visitor)
        {
            return visitor.VisitLiteralExpression<double>(this);
        }
    }

    public class SBQLBooleanLiteral : SBQLLiteralExpression<bool>
    {
        protected internal override object Accept(SBQLExpressionVisitor visitor)
        {
            return visitor.VisitLiteralExpression<bool>(this);
        }
    }

    public class SBQLNameExpression : SBQLExpression
    {
        public enum NameType
        {
            Param,
            Member,
            //Aux - bind to the name that should not generate parameter expression during conversion - it should be consumed by generation basing on pattern
            Aux,
            Method,
            //local variable (a mamber of "constant" - anonymous class)
            Local
        }

        public String Name { get; set; }       
        public NameType NameSource { get; set; }
        public MemberInfo Member { get; set; }
        

        public override string ToString()
        {
            return Name.ToString();
        }

        protected internal override object Accept(SBQLExpressionVisitor visitor)
        {
            return visitor.VisitNameExpression(this);
        }

    }

    public abstract class SBQLUnaryExpression : SBQLExpression
    {
        internal SBQLUnaryExpression() { }

        public SBQLExpression Operand { get { return operand; } set { operand = value; value.Parent = this; } }
        private SBQLExpression operand;

    }

    public class SBQLUnaryAlgebraicExpression : SBQLUnaryExpression
    {
        public enum UnaryExpressionType
        {
            Sum, Avg, Max, Min, Negate,
            Exists,
            Count
        }



        static readonly Dictionary<UnaryExpressionType, String> type2string = new Dictionary<UnaryExpressionType, string>();
        static SBQLUnaryAlgebraicExpression()
        {

            type2string.Add(UnaryExpressionType.Sum, " sum");
            type2string.Add(UnaryExpressionType.Avg, " avg");
            type2string.Add(UnaryExpressionType.Max, " max");
            type2string.Add(UnaryExpressionType.Min, " min");
            type2string.Add(UnaryExpressionType.Negate, " -");
            type2string.Add(UnaryExpressionType.Exists, " exists");
            type2string.Add(UnaryExpressionType.Count, " count");

        }

        public UnaryExpressionType OperatorType { get; set; }

        public String OperatorName { get { return type2string[OperatorType]; } }
        public MethodInfo Method { get; set; }

        public override string ToString()
        {
            return OperatorName + "(" + Operand.ToString() + ")";
        }

        protected internal override object Accept(SBQLExpressionVisitor visitor)
        {
            return visitor.VisitUnaryAlgebraicExpression(this);
        }
    }

    public abstract class SBQLBinaryExpression : SBQLExpression
    {


        public SBQLExpression LeftOperand { get { return leftOperand; } set { leftOperand = value; value.Parent = this; } }
        public SBQLExpression RightOperand { get { return rightOperand; } set { rightOperand = value; value.Parent = this; } }

        private SBQLExpression leftOperand;
        private SBQLExpression rightOperand;


    }

    public class SBQLBinaryAlgebraicExpression : SBQLBinaryExpression
    {
        public enum BinaryExpressionType
        {
            GreaterThan, GreaterEqualThan, LowerThan, LowerEqualThan, Equal, NotEqual, Add, Subtract, Multiply, Divide,  Intersect, Minus,
            Contains
        }

        static readonly Dictionary<BinaryExpressionType, String> type2string = new Dictionary<BinaryExpressionType, string>();
        static SBQLBinaryAlgebraicExpression()
        {
            type2string.Add(BinaryExpressionType.GreaterEqualThan, " >= ");
            type2string.Add(BinaryExpressionType.GreaterThan, " > ");
            type2string.Add(BinaryExpressionType.LowerEqualThan, " <= ");
            type2string.Add(BinaryExpressionType.LowerThan, " < ");
            type2string.Add(BinaryExpressionType.Equal, " = ");
            type2string.Add(BinaryExpressionType.NotEqual, " != ");
            type2string.Add(BinaryExpressionType.Add, " + ");
            type2string.Add(BinaryExpressionType.Subtract, " - ");
            type2string.Add(BinaryExpressionType.Multiply, " * ");
            type2string.Add(BinaryExpressionType.Divide, " / ");
           
            type2string.Add(BinaryExpressionType.Intersect, " intersect ");
            type2string.Add(BinaryExpressionType.Minus, " minus ");
            type2string.Add(BinaryExpressionType.Contains, "  contains ");
        }

        public BinaryExpressionType OperatorType { get; set; }

        public String OperatorName { get { return type2string[OperatorType]; } }
        public MethodInfo Method { get; set; }

        public override string ToString()
        {
            return " (" + LeftOperand.ToString() + ") " + OperatorName + " (" + RightOperand.ToString() + ") ";
        }

        protected internal override object Accept(SBQLExpressionVisitor visitor)
        {
            return visitor.VisitBinaryAlgebraicExpression(this);
        }
    }

    public class SBQLNonAlgebraicExpression : SBQLBinaryExpression
    {
        internal SBQLNonAlgebraicExpression() { }
        public enum NonAlgebraicOperator
        {
            WHERE, DOT, JOIN,
            FORALL,
            FORSOME
        }

        public NonAlgebraicOperator OperatorType { get; set; }

        public String OperatorName
        {
            get
            {
                string name;
                switch (OperatorType)
                {
                    case NonAlgebraicOperator.WHERE:
                        name = "where";
                        break;
                    case NonAlgebraicOperator.DOT:
                        name = ".";
                        break;
                    case NonAlgebraicOperator.JOIN:
                        name = "join";
                        break;
                    case NonAlgebraicOperator.FORALL:
                        name = "forall";
                        break;
                    case NonAlgebraicOperator.FORSOME:
                        name = "forsome";
                        break;
                    default:
                        throw new NotImplementedException();

                }
                return name;
            }
        }

        protected internal override object Accept(SBQLExpressionVisitor visitor)
        {
            return visitor.VisitNonAlgebraicExpression(this);
        }

        public override string ToString()
        {
            return " (" + LeftOperand.ToString() + ") " + OperatorName + " (" + RightOperand.ToString() + ") ";
        }
    }



    public class SBQLAuxNameExpression : SBQLUnaryExpression
    {
        internal SBQLAuxNameExpression() { }
        public String OperatorName { get { return Group ? "groupas" : "as"; } }
        public String Name { get; set; }
        public bool Group { get; set; }

        public bool IsParameterRepresentation { get; set; }
        
        

        public override string ToString()
        {
            return " (" + Operand.ToString() + ") " + OperatorName + " " + Name;
        }

        protected internal override object Accept(SBQLExpressionVisitor visitor)
        {
            return visitor.VisitAuxNameExpression(this);
        }
    }

    public class SBQLProcedureCallExpression : SBQLExpression
    {
        
        public SBQLNameExpression Expression {get; set; }
        public MethodInfo MethodInfo { get; set; }
        public System.Collections.ObjectModel.ReadOnlyCollection<SBQLExpression> Arguments { get { return parameters.AsReadOnly(); } }
        public void AddArgument(SBQLExpression param)
        {
            if (param == null)
                throw new NullReferenceException("param expression for procedure call " + Expression.Name);
            parameters.Add(param);
        }
        

        public int ParamsCount()
        {
            return parameters.Count;
        }
        private List<SBQLExpression> parameters = new List<SBQLExpression>();
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Expression.Name);
            builder.Append("(");
            foreach (SBQLExpression p in Arguments)
            {
                builder.Append(p.ToString());
                builder.Append(",");
            }
            builder.Append(")");
            return builder.ToString();
        }

        protected internal override object Accept(SBQLExpressionVisitor visitor)
        {
            return visitor.VisitProcedureCallExpression(this);
        }
    }

    public class SBQLCreateExptession : SBQLExpression
    {
        public SBQLExpression Operand { get; set; }
        protected internal override object Accept(SBQLExpressionVisitor visitor)
        {
            return visitor.VisitCreateExpression(this);
        }
    }

    public abstract class SBQLWrapperExpression<T> : SBQLExpression
    {
        public T Expression { get; set; }

        protected internal override object Accept(SBQLExpressionVisitor visitor)
        {
            return visitor.VisitWrapperExpression<T>(this);
        }

        public override string ToString()
        {
            return Expression.ToString();
        }
    }
    public abstract class SBQLExpressionVisitor
    {
        public virtual object Visit(SBQLExpression expr)
        {
            return expr.Accept(this);
        }


        protected internal abstract object VisitLiteralExpression<T>(SBQLLiteralExpression<T> literal);
        protected internal abstract object VisitNameExpression(SBQLNameExpression literal);

        protected internal abstract object VisitUnaryAlgebraicExpression(SBQLUnaryAlgebraicExpression expr);

        protected internal abstract object VisitBinaryAlgebraicExpression(SBQLBinaryAlgebraicExpression expr);

        protected internal abstract object VisitAuxNameExpression(SBQLAuxNameExpression literal);

        protected internal abstract object VisitNonAlgebraicExpression(SBQLNonAlgebraicExpression expr);
        protected internal abstract object VisitProcedureCallExpression(SBQLProcedureCallExpression expr);

        protected internal abstract object VisitWrapperExpression<T>(SBQLWrapperExpression<T> expr);

        protected internal abstract object VisitCreateExpression(SBQLCreateExptession expr);
        

    }

}
