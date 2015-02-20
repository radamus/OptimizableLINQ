using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace OptimizableLINQ
{
    using SBQL.Environment;
    class BindingLevelCalculator : ExpressionVisitor
    {

        class ConstantFinder : ExpressionVisitor
        {
            internal ConstantExpression[] Find(Expression expr)
            {
                Visit(expr);
                return names.ToArray();
            }
            List<ConstantExpression> names = new List<ConstantExpression>();
            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (!node.Type.IsPrimitive)
                    names.Add(node);
                return base.VisitConstant(node);
            }
        }

        class BindingLevelCalculatorENVS : ENVS<Expression>
        {

            Stack<BindingLevelsInfo.Operator> operatorContext = new Stack<BindingLevelsInfo.Operator>();

            public void CreateOperatorContext(BindingLevelsInfo.Operator oper)
            {
                operatorContext.Push(oper);
            }

            public void DestroyOperatorContext(BindingLevelsInfo.Operator expr)
            {
                if (operatorContext.Count < 1)
                    throw new OptimizationException("empty operator context ");
                BindingLevelsInfo.Operator oper = operatorContext.Pop();
                if (!expr.Equals(oper))
                    throw new OptimizationException("wrong operator context" + expr);
            }



            public BindingLevelsInfo.Operator getOperatorContext()
            {
                if (operatorContext.Count < 1)
                    throw new OptimizationException("no operator context ");
                return operatorContext.Peek();
            }



            public void Push(ParameterExpression pe)
            {
                if (pe.Name == null)
                {

                }
                base.PushBinder(new Binder<Expression>() { Name = pe.Name, Value = pe });
            }


        }





        BindingLevelCalculatorENVS envs = new BindingLevelCalculatorENVS();
        BindingLevelsInfo paramBindingInfo = new BindingLevelsInfo();


        public BindingLevelsInfo Extract(Expression expression)
        {
            //base section

            envs.CreateOperatorContext(paramBindingInfo.GetBase());
            envs.CreateEnv();
            foreach (ConstantExpression ce in new ConstantFinder().Find(expression).Distinct())
                envs.PushBinder(new Binder<Expression>() { Name = ce.Value.ToString(), Value = ce });
            Visit(expression);
            envs.DestroyEnv();
            envs.DestroyOperatorContext(paramBindingInfo.GetBase());
            return paramBindingInfo;
        }

        public static string foo()
        {
            return "";
        }
        protected override Expression VisitLambda<T>(Expression<T> node)
        {

            envs.CreateEnv();

            foreach (ParameterExpression pe in node.Parameters)
            {
                envs.Push((pe));
            }
            this.Visit(node.Body);
            envs.DestroyEnv();
            return node;

        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            BindingLevelCalculatorENVS.BindingInfo<Expression> result = envs.Bind(node.Name);
            if (!result.Found)
                throw new OptimizationException("unable to bind parameter " + node.Name);

            paramBindingInfo.AddBinding(envs.getOperatorContext(), node, result.Section);

            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (!isOperator(node))
            {
                return base.VisitMethodCall(node);
            }
            LinqInfo.LINQOperator linqOperator = LinqInfo.LINQOperator.GetLinqOperator(node.Method);
            Visit(node.Arguments[0]);
            BindingLevelsInfo.Operator o = null;
            if (linqOperator.IsLambda)
            {
                 o = paramBindingInfo.AddOperator(node, envs.Size);
                envs.CreateOperatorContext(o);
            }
            for (int i = 1; i < node.Arguments.Count; i++)
            {

                Visit(node.Arguments[i]);

            }
            if (linqOperator.IsLambda)
            {
                envs.DestroyOperatorContext(o);
            }

            return node;
        }

        private bool isOperator(MethodCallExpression node)
        {

            return LinqInfo.LINQOperator.IsLinqOperatorCall(node.Method);

        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            
            if (!((node.Type.IsPrimitive) || (node.Value as String) != null) )
            {
                BindingLevelCalculatorENVS.BindingInfo<Expression> result = envs.Bind(node.Value.ToString());
                paramBindingInfo.AddBinding(envs.getOperatorContext(), node, result.Section);
            }
            return base.VisitConstant(node);
        }
    }

    internal class BindingLevelsInfo
    {

        internal class Operator
        {

            internal Operator(MethodCallExpression operatorExpr)
            {
                Id = operatorExpr.GetHashCode();
                OperatorExpr = operatorExpr;
            }

            internal int Id { get; private set; }
            internal MethodCallExpression OperatorExpr { get; private set; }
            internal int OpenedSectionNumber { get; set; }

            
            
                                    
            public override bool Equals(object obj)
            {
                Operator o;
                if ((o = (obj as Operator)) != null)
                {
                    if (OperatorExpr == null)
                        return o.OperatorExpr == null;
                    return OperatorExpr.Equals(o.OperatorExpr);
                }
                return false;
            }

            public override int GetHashCode()
            {
                if (OperatorExpr != null)
                    return OperatorExpr.GetHashCode();
                return -1;
            }

            static void foo() { }

            internal static Operator BaseSectionOperator = new Operator(Expression.Call(typeof(Operator), "foo", new Type[] { }, new Expression[] { })) { OpenedSectionNumber = 0 };
            public override string ToString()
            {
                string result = "Id: "+ Id + " Expression: " + this.OperatorExpr.Method.Name + ", section: " + this.OpenedSectionNumber;
                
                return result;
            }
        }

        internal Dictionary<Expression, Operator> bindingLevels = new Dictionary<Expression, Operator>();
        Dictionary<MethodCallExpression, Operator> operators = new Dictionary<MethodCallExpression, Operator>();
        List<Operator> all = new List<Operator>();

        internal BindingLevelsInfo()
        {
            AddBaseOperator();
        }

        internal bool isOperator(MethodCallExpression expr)
        {
            return operators.ContainsKey(expr);
        }
        internal Operator AddOperator(MethodCallExpression mce, int stackSize)
        {
            if (!operators.ContainsKey(mce))
            {
                operators[mce] = new Operator(mce) { OpenedSectionNumber = stackSize };
                all.Add(operators[mce]);
            }
            return operators[mce];
        }
        

        void AddBaseOperator()
        {
            if (!operators.ContainsKey(Operator.BaseSectionOperator.OperatorExpr))
                operators[Operator.BaseSectionOperator.OperatorExpr] = Operator.BaseSectionOperator;
        }

        internal void AddBinding(BindingLevelsInfo.Operator oper, Expression bindExpression, int sectionNumber)
        {
            if (!this.bindingLevels.ContainsKey(bindExpression))
            {
                BindingLevelsInfo.Operator coper = this.operators.Values.Where(o => o.OpenedSectionNumber == sectionNumber).FirstOrDefault();
                if (coper == null)
                    throw new OptimizationFatalException("unable to find operator opening the section: '" + sectionNumber + "' in the context of binding '" + bindExpression + "'");
                this.bindingLevels[bindExpression] = coper;
            }
        }

        
        

        internal int getBindinglevelForBindExpression(Expression node)
        {
            if (!bindingLevels.ContainsKey(node))
            {
                throw new OptimizationException("unknown expression " + node);
            }
            return bindingLevels[node].OpenedSectionNumber;
        }

        internal Operator GetOperator(MethodCallExpression methodCallExpression)
        {
            if (!operators.ContainsKey(methodCallExpression))
                throw new OptimizationException("unknown operator " + methodCallExpression.Method.Name);

            return operators[methodCallExpression];
        }



        internal BindingLevelsInfo.Operator GetBase()
        {
            return Operator.BaseSectionOperator;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (Operator o in all)
            {
                builder.AppendLine(o.ToString());
            }
            foreach (Expression e in this.bindingLevels.Keys)
            {
                if(e.NodeType == ExpressionType.Parameter)
                    builder.AppendLine((e as ParameterExpression).Name + " : " + bindingLevels[e]);
                else if(e.NodeType == ExpressionType.Constant)
                    builder.AppendLine("["+ (e as ConstantExpression).Value +"]: " + bindingLevels[e]);
            }
            return builder.ToString();
        }
    }

    public class BindingLevelsPrinter : ExpressionVisitor
    {
        StringBuilder queryBuilder = new StringBuilder();
        StringBuilder levelsBuilder = new StringBuilder();
        BindingLevelsInfo bindingInfo;
        Stack<BindingLevelsInfo.Operator> operatorContext = new Stack<BindingLevelsInfo.Operator>();

        internal String[] Print(Expression expression, BindingLevelsInfo bindingInfo)
        {
            this.bindingInfo = bindingInfo;
            operatorContext.Push(bindingInfo.GetBase());
            Visit(expression);

            return new String[] { expression.ToString(), levelsBuilder.ToString() };
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (!this.bindingInfo.isOperator(node))
            {
                return base.VisitMethodCall(node);
            }

            Visit(node.Arguments[0]);
            if (node.Arguments.Count > 1)
            {
                this.levelsBuilder.Append(node.Method.Name + "( ");
                BindingLevelsInfo.Operator oper = this.bindingInfo.GetOperator(node);
                operatorContext.Push(oper);
                for (int i = 1; i < node.Arguments.Count; i++)
                {
                    Visit(node.Arguments[i]);
                }
                operatorContext.Pop();
                this.levelsBuilder.Append(" ) ");
            }
            return node;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            BindingLevelsInfo.Operator oper = operatorContext.Peek();

            this.levelsBuilder.Append(oper.OpenedSectionNumber + " => ");

            Visit(node.Body);

            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            BindingLevelsInfo.Operator oper = operatorContext.Peek();
            this.levelsBuilder.Append(this.bindingInfo.getBindinglevelForBindExpression(node) + "[" + node.Name + "]");
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            BindingLevelsInfo.Operator oper = operatorContext.Peek();
            if(!node.Type.IsPrimitive)
                this.levelsBuilder.Append(this.bindingInfo.getBindinglevelForBindExpression(node) + "[" + "]");
            return node;
        }
    }

}
