using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Text;

namespace LINQ_SBQLConverters
{
    using SBQL.Expressions;
    using LinqInfo;
    
    class Linq2SBQLConverter : ExpressionVisitor
    {


        enum State
        {
            ROOT, LPARAM, RPARAM, PARAM, LAMBDABODY
        }


        class MethodCallArguments
        {
            public SBQLExpression Expression { get; set; }
            public LambdaInfo Lambda { get; set; }

        }

        class StackElem
        {

          
            internal LINQOperator LinqOperator { get; set; }
            internal SBQLExpression Expression { get; set; }
            internal LambdaInfo Lambda { get; set; }
        }

        private Stack<StackElem> stateStack = new Stack<StackElem>();






        public Linq2SBQLConverter()
        {
            stateStack.Push(new StackElem());
        }

        public SBQLLambdaExpression Convert(LambdaExpression expr)
        {
            stateStack.Clear();
            stateStack.Push(new StackElem());
          
            this.Visit(expr);
            StackElem result = stateStack.Pop();
            SBQLLambdaExpression root = new SBQLLambdaExpression() { Body = result.Expression};

            root.Lambda = result.Lambda;

            return root;
        }


        public SBQLExpression Convert(Expression expr)
        {
            stateStack.Clear();
            stateStack.Push(new StackElem());

            this.Visit(expr);
            StackElem result = stateStack.Pop();
            
            return result.Expression;
        }


        protected override Expression VisitNew(NewExpression node)
        {
            SBQLLinqExpressionWrapper wrapper = new SBQLLinqExpressionWrapper();
            stateStack.Peek().Expression = wrapper;
            wrapper.Expression = node;
            wrapper.ResultType = node.Type;
            return node;
        }

        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {
            return base.VisitMemberBinding(node);
        }
        protected override Expression VisitMethodCall(MethodCallExpression mce)
        {
            SBQLExpression result;
            if (mce.Object == null ) //static extension method
            {

                if (!LINQOperator.IsLinqOperatorCall(mce.Method))
                    throw new NotSupportedException("unknown linq operator " + mce.Method.Name + " for arguments number " + mce.Arguments.Count);
                LINQOperator linqoperator = LINQOperator.GetLinqOperator(mce.Method);
                if (linqoperator.IsLambda)
                {
                    MethodCallArguments[] arguments = new MethodCallArguments[mce.Arguments.Count];
                    
                    stateStack.Push(new StackElem());
                    this.Visit(mce.Arguments[0]);
                    
                    arguments[0] = new MethodCallArguments() { Expression = stateStack.Pop().Expression };                        
                    if (arguments[0].Expression == null)
                        throw new NullReferenceException("evaluation of MethodCallExpression.Arguments[0]: " + mce.Arguments[0].NodeType + " " + mce.Arguments[0].Type);


                    for (int i = 1; i < mce.Arguments.Count; i++)
                    {
                        stateStack.Push(new StackElem() { LinqOperator = linqoperator });
                        this.Visit(mce.Arguments[i]);
                        StackElem elem = stateStack.Pop();
                        arguments[i] = new MethodCallArguments() { Expression = elem.Expression, Lambda = elem.Lambda };
                        if (arguments[i].Expression == null)
                            throw new NullReferenceException("evaluation of MethodCallExpression.Arguments[: "+ i + "] " + mce.Arguments[0].NodeType + " " + mce.Arguments[0].Type);
                    }

                    
                    result = generateSBQLExpressionForLinqLambdaOperator(linqoperator, mce, arguments);

                }
                else
                {
                    List<SBQLExpression> operands = new List<SBQLExpression>();
                    foreach (Expression arg in mce.Arguments)
                    {
                        stateStack.Push(new StackElem());
                        this.Visit(arg);
                        StackElem elem = stateStack.Pop();
                        if (elem.Expression == null)
                            throw new NullReferenceException("evaluation of argument: " + arg.NodeType + " " + arg.Type + " for " + mce.Method.Name);
                        operands.Add(elem.Expression);
                    }
                    result = this.generateSBQLExpressionForLinqAlgebraicOperator(linqoperator, mce.Method, operands);
                }

            }
            else
            {
                if (mce.Object != null) //instance method
                {
                    stateStack.Push(new StackElem());
                    this.Visit(mce.Object);
                    StackElem elem = stateStack.Pop();

                    SBQLNonAlgebraicExpression dotExpr = SBQLExpression.NonAlgebraicExpression(SBQLNonAlgebraicExpression.NonAlgebraicOperator.DOT, mce.Type);
                    dotExpr.LeftOperand = elem.Expression;
                    SBQLProcedureCallExpression procExpr = SBQLExpression.ProcedureCallExpression();
                    procExpr.MethodInfo = mce.Method;
                    SBQLNameExpression nexpr = SBQLExpression.NameExpression(mce.Method.Name, mce.Type);
                    nexpr.NameSource = SBQLNameExpression.NameType.Method;
                    procExpr.Expression = nexpr;
                    foreach (Expression p in mce.Arguments)
                    {
                        stateStack.Push(new StackElem());
                        this.Visit(mce.Object);
                        elem = stateStack.Pop();
                        procExpr.AddArgument(elem.Expression);

                    }
                    dotExpr.RightOperand = procExpr;
                    result = dotExpr;
                }
                else
                {  //static method - not an extension - don't know what to do
                    result = new SBQLLinqExpressionWrapper { Expression = mce };
                }
            }

            stateStack.Peek().Expression = result;
            return mce;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Quote)
            {//we must skip this node for lambda expression so only check invariats
                StackElem elem = stateStack.Peek();
                if (!elem.LinqOperator.IsLambda)
                {
                    throw new NotImplementedException("lambda expression unsupported for non-lambda operators " + elem.LinqOperator.Name);
                }
               
                this.Visit(node.Operand);

            }
            else
            {
                stateStack.Push(new StackElem());
                this.Visit(node.Operand);
                StackElem elem = stateStack.Pop();
                stateStack.Peek().Expression = generateSBQLExpressionForLinqUnaryExpression(node, elem.Expression);
            }
            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            
            stateStack.Push(new StackElem());
            this.Visit(node.Left);
            SBQLExpression lexpr = stateStack.Pop().Expression;

            stateStack.Push(new StackElem());
            this.Visit(node.Right);
            SBQLExpression rexpr = stateStack.Pop().Expression;
            stateStack.Peek().Expression = generateSBQLExpressionForLinqBinaryExpression(node, lexpr, rexpr);
            return node;

        }
        protected override Expression VisitConstant(ConstantExpression node)
        {

            if (node.Type.IsPrimitive)
            {
                stateStack.Peek().Expression = SBQLExpression.LiteralExpression(node.Type, node.Value);
            }
            else
                stateStack.Peek().Expression = new SBQLLinqExpressionWrapper { Expression = node };


            return node;
        }
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            StackElem elem = stateStack.Peek();
            LambdaInfo lambda = new LambdaInfo();
            lambda.ResultType = node.ReturnType;
            
            foreach (ParameterExpression pe in node.Parameters)
            {
                lambda.parameters.Add(new LambdaInfo.LambdaParameter { Type = pe.Type, Name = pe.Name});
            }
            elem.Lambda = lambda;
            stateStack.Push(new StackElem());
            this.Visit(node.Body);
            StackElem bodyElem = stateStack.Pop();
            stateStack.Peek().Expression = bodyElem.Expression;
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            //member access expression should be turn into sbql navigation (dot)
            //visit node.Expression to evaluate left operand 
            stateStack.Push(new StackElem());
            this.Visit(node.Expression);
            StackElem elem = stateStack.Pop();
            SBQLExpression lexpr = elem.Expression;
            SBQLNonAlgebraicExpression dotExpr = SBQLExpression.NonAlgebraicExpression(SBQLNonAlgebraicExpression.NonAlgebraicOperator.DOT, node.Type);
            SBQLNameExpression nexpr = SBQLExpression.NameExpression(node.Member.Name, node.Type);
            if (node.Expression.NodeType == ExpressionType.Constant)
            {
                nexpr.NameSource = SBQLNameExpression.NameType.Local;
            }
            else
            {
                nexpr.NameSource = SBQLNameExpression.NameType.Member;
            }
            
            nexpr.Member = node.Member;

            dotExpr.LeftOperand = lexpr;
            dotExpr.RightOperand = nexpr;
            stateStack.Peek().Expression = dotExpr;
            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            //if we are here it means that we bind to the parameter name 
            SBQLNameExpression nexpr = SBQLExpression.NameExpression(node.Name, node.Type);

            nexpr.NameSource = SBQLNameExpression.NameType.Param;
            stateStack.Peek().Expression = nexpr;
            return node;
        }

        private SBQLExpression generateSBQLExpressionForLinqBinaryExpression(BinaryExpression expression, SBQLExpression sBQLLeftExpression, SBQLExpression sBQLRightExpression)
        {
            if (sBQLLeftExpression == null)
                throw new NullReferenceException("binary operator left operand");
            if (sBQLRightExpression == null)
                throw new NullReferenceException("binary operator right operand");
            SBQLBinaryAlgebraicExpression result = null;
            if (LINQOperators.linqExpressionType2sbqlBinary.ContainsKey(expression.NodeType))
            {
                SBQLBinaryAlgebraicExpression.BinaryExpressionType exprType = LINQOperators.linqExpressionType2sbqlBinary[expression.NodeType];
                result = SBQLExpression.BinaryAlgebraicExpression(exprType, expression.Type);
                result.LeftOperand = sBQLLeftExpression;
                result.RightOperand = sBQLRightExpression;
                result.Method = expression.Method;
            }
            else
            {
                throw new NotImplementedException("unimplemented binary operator: " + expression.NodeType.ToString());
            }

            return result;

        }

        private SBQLExpression generateSBQLExpressionForLinqUnaryExpression(UnaryExpression expression, SBQLExpression sBQLExpression)
        {
            SBQLUnaryAlgebraicExpression result = null;
            if (LINQOperators.linqExpressionType2sbqlUnary.ContainsKey(expression.NodeType))
            {
                SBQLUnaryAlgebraicExpression.UnaryExpressionType exprType = LINQOperators.linqExpressionType2sbqlUnary[expression.NodeType];
                result = SBQLExpression.UnaryAlgebraicExpression(exprType, expression.Type);

                result.Operand = sBQLExpression;
                result.Method = expression.Method;
                return result;
            }
            else
            {
                throw new NotImplementedException("unimplemented unary operator: " + expression.ToString());
            }



        }

        private SBQLExpression generateSBQLExpressionForLinqLambdaOperator(LINQOperator linqOper, MethodCallExpression mce, MethodCallArguments[] arguments)
        {
            SBQLExpression result;
            switch (linqOper.Type)
            {
                case LINQOperator.LINQOperatorType.Select:
                    //source.Select(selem => selectorBody) -> ((source as selem).selectorBody)
                    LambdaInfo lambda = arguments[1].Lambda;
                    SBQLExpression lexpr = arguments[0].Expression;
                    SBQLExpression rexpr = arguments[1].Expression;
                    SBQLNonAlgebraicExpression dotExpr = generateSBQLExpressionForLinqSelect(lambda, lexpr, rexpr);                 
                    result = dotExpr;
                    break;
                case LINQOperator.LINQOperatorType.Where:
                    //source.Where(selem => predicateBody) -> ((source as selem) where predicateBody).selem
                    lambda = arguments[1].Lambda;
                    lexpr = arguments[0].Expression;
                    rexpr = arguments[1].Expression;
                    
                    SBQLNonAlgebraicExpression expr = SBQLExpression.NonAlgebraicExpression(SBQLNonAlgebraicExpression.NonAlgebraicOperator.WHERE, lambda.ResultType); //the type is fake - we have a binder here
                    SBQLAuxNameExpression nExpr = SBQLExpression.AuxNameExpression(lambda.parameters[0].Name, false, lambda.parameters[0].Type);
                    nExpr.IsParameterRepresentation = true;
                    nExpr.Operand = lexpr;
                    expr.LeftOperand = nExpr;
                    expr.RightOperand = rexpr;
                    dotExpr = SBQLExpression.NonAlgebraicExpression(SBQLNonAlgebraicExpression.NonAlgebraicOperator.DOT, lambda.ResultType); //here is the right type 
                    dotExpr.LeftOperand = expr;
                    SBQLNameExpression nameExpr = SBQLExpression.NameExpression(lambda.parameters[0].Name, lambda.parameters[0].Type);
                    nameExpr.NameSource = SBQLNameExpression.NameType.Aux;
                    dotExpr.RightOperand = nameExpr;
                    result = dotExpr;
                    break;
                case LINQOperator.LINQOperatorType.Join:
                    //outer.Join(inner, oelem => outerKeySelectorBody, ielem => innerKeySelectorBody, (soelem, sielem) => resultSelectorBody)
                //  ((outer as oelem join inner as ielem) where outerKeySelectorBody = innerKeySelectorBody).(oelem as soelem join ielem as sielem).resultSelectorBody
                    SBQLExpression outerExpr = arguments[0].Expression;
                    SBQLExpression innerExpr = arguments[1].Expression;
                    LambdaInfo oelemLambda = arguments[2].Lambda;
                    SBQLExpression outerKeySelectorBodyExpr = arguments[2].Expression;
                    LambdaInfo ielemLambda = arguments[3].Lambda;
                    SBQLExpression innerKeySelectorBodyExpr = arguments[3].Expression;
                    LambdaInfo soelem_sielemLambda = arguments[4].Lambda;
                    SBQLExpression resultSelectorBodyExpr = arguments[4].Expression;
                    SBQLNonAlgebraicExpression resultDot  = SBQLExpression.NonAlgebraicExpression(SBQLNonAlgebraicExpression.NonAlgebraicOperator.DOT, soelem_sielemLambda.ResultType);
                    //type?
                    SBQLNonAlgebraicExpression innerDot = SBQLExpression.NonAlgebraicExpression(SBQLNonAlgebraicExpression.NonAlgebraicOperator.DOT, soelem_sielemLambda.ResultType);
                    //type
                    SBQLNonAlgebraicExpression selectedjoinExpr = SBQLExpression.NonAlgebraicExpression(SBQLNonAlgebraicExpression.NonAlgebraicOperator.JOIN, soelem_sielemLambda.ResultType);
                    SBQLAuxNameExpression soelemAuxExpr = SBQLExpression.AuxNameExpression(soelem_sielemLambda.parameters[0].Name, false, soelem_sielemLambda.parameters[0].Type);
                    SBQLAuxNameExpression sielemAuxExpr = SBQLExpression.AuxNameExpression(soelem_sielemLambda.parameters[1].Name, false, soelem_sielemLambda.parameters[1].Type);

                    //type?
                    SBQLNonAlgebraicExpression whereExpr = SBQLExpression.NonAlgebraicExpression(SBQLNonAlgebraicExpression.NonAlgebraicOperator.WHERE, soelem_sielemLambda.ResultType);
                    //type?
                    SBQLNonAlgebraicExpression initjoinExpr = SBQLExpression.NonAlgebraicExpression(SBQLNonAlgebraicExpression.NonAlgebraicOperator.JOIN, soelem_sielemLambda.ResultType);
                    SBQLAuxNameExpression oelemAuxExpr = SBQLExpression.AuxNameExpression(oelemLambda.parameters[0].Name, false, oelemLambda.parameters[0].Type);
                    SBQLAuxNameExpression ielemAuxExpr = SBQLExpression.AuxNameExpression(ielemLambda.parameters[0].Name, false, ielemLambda.parameters[0].Type);

                    oelemAuxExpr.Operand = outerExpr;
                    ielemAuxExpr.Operand = innerExpr;
                    initjoinExpr.LeftOperand = oelemAuxExpr;
                    initjoinExpr.RightOperand = ielemAuxExpr;

                    SBQLBinaryAlgebraicExpression equalityExpr =  SBQLExpression.BinaryAlgebraicExpression(SBQLBinaryAlgebraicExpression.BinaryExpressionType.Equal, typeof(bool));
                    equalityExpr.LeftOperand = outerKeySelectorBodyExpr;
                    equalityExpr.RightOperand = innerKeySelectorBodyExpr;
                    whereExpr.LeftOperand = initjoinExpr;
                    whereExpr.RightOperand = equalityExpr;

                    SBQLNameExpression oelemNameBindExpr = SBQLExpression.NameExpression(oelemLambda.parameters[0].Name, oelemLambda.parameters[0].Type);
                    oelemNameBindExpr.NameSource = SBQLNameExpression.NameType.Aux;
                    soelemAuxExpr.Operand = oelemNameBindExpr;
                    
                    SBQLNameExpression ielemNameBindExpr = SBQLExpression.NameExpression(ielemLambda.parameters[0].Name, ielemLambda.parameters[0].Type);
                    ielemNameBindExpr.NameSource = SBQLNameExpression.NameType.Aux;
                    sielemAuxExpr.Operand = ielemNameBindExpr;
                    selectedjoinExpr.LeftOperand = soelemAuxExpr;
                    selectedjoinExpr.RightOperand = sielemAuxExpr;
                    innerDot.LeftOperand = whereExpr;
                    innerDot.RightOperand = selectedjoinExpr;
                    resultDot.LeftOperand = innerDot;
                    resultDot.RightOperand = resultSelectorBodyExpr;
                    result = resultDot;
                    break;
                case LINQOperator.LINQOperatorType.Sum:
                    goto case LINQOperator.LINQOperatorType.Min;
                case LINQOperator.LINQOperatorType.Average:
                    goto case LINQOperator.LINQOperatorType.Min;
                case LINQOperator.LINQOperatorType.Max:
                    goto case LINQOperator.LINQOperatorType.Min;
                case LINQOperator.LINQOperatorType.Min:
                    //  source.[Operator](selem => selectorBody) -> [operator] ((source as selem).selem.selectorBody)
                    lambda = arguments[1].Lambda;
                    lexpr = arguments[0].Expression;
                    rexpr = arguments[1].Expression;
                    result = generateSBQLExpressionForLinqUnaryAlgebraicOperatorWithProjectionLambda(linqOper, mce.Method, lambda, new SBQLExpression[] { lexpr, rexpr });
                    break;
                case LINQOperator.LINQOperatorType.All:
                    lambda = arguments[1].Lambda;
                    lexpr = arguments[0].Expression;
                    rexpr = arguments[1].Expression;
                    result = generateSBQLExpressionForLinqQuantifier(SBQLNonAlgebraicExpression.NonAlgebraicOperator.FORALL, lambda, lexpr, rexpr);
                    break;
                case LINQOperator.LINQOperatorType.Any:
                    lambda = arguments[1].Lambda;
                    lexpr = arguments[0].Expression;
                    rexpr = arguments[1].Expression;
                    result = generateSBQLExpressionForLinqQuantifier(SBQLNonAlgebraicExpression.NonAlgebraicOperator.FORSOME, lambda, lexpr, rexpr);
                    break;
                default:
                    throw new NotImplementedException("generateSBQLExpressionForLinqLambdaOperator - unknown LINQOperator " + linqOper.Name);
            }


            return result;
        }

        private SBQLExpression generateSBQLExpressionForLinqQuantifier(SBQLNonAlgebraicExpression.NonAlgebraicOperator nonAlgebraicOperator, LambdaInfo lambda, SBQLExpression lexpr, SBQLExpression rexpr)
        {
            SBQLNonAlgebraicExpression quantifierExpr = SBQLExpression.NonAlgebraicExpression(nonAlgebraicOperator, typeof(bool));
            SBQLAuxNameExpression nExpr = SBQLExpression.AuxNameExpression(lambda.parameters[0].Name, false, lambda.parameters[0].Type);
            nExpr.Operand = lexpr;
            nExpr.IsParameterRepresentation = true;

            quantifierExpr.LeftOperand = nExpr;
            quantifierExpr.RightOperand = rexpr;
            return quantifierExpr;
        }

        private SBQLNonAlgebraicExpression generateSBQLExpressionForLinqSelect(LambdaInfo lambda, SBQLExpression lexpr, SBQLExpression rexpr)
        {
            SBQLNonAlgebraicExpression dotExpr = SBQLExpression.NonAlgebraicExpression(SBQLNonAlgebraicExpression.NonAlgebraicOperator.DOT, lambda.ResultType);
            SBQLAuxNameExpression nExpr = SBQLExpression.AuxNameExpression(lambda.parameters[0].Name, false, lambda.parameters[0].Type);
            nExpr.Operand = lexpr;
            nExpr.IsParameterRepresentation = true;

            dotExpr.LeftOperand = nExpr;
            dotExpr.RightOperand = rexpr;
            return dotExpr;
        }

        private SBQLExpression generateSBQLExpressionForLinqUnaryAlgebraicOperatorWithProjectionLambda(LINQOperator linqOper, MethodInfo methodInfo, LambdaInfo lambda, SBQLExpression[] sBQLExpression)
        {
            //  source.Average(selem => selectorBody) -> avg ((source as selem).selectorBody)
            SBQLUnaryAlgebraicExpression unaryExpr = SBQLExpression.UnaryAlgebraicExpression(LINQOperators.linqOperator2sbqlUnary[linqOper.Type], methodInfo.ReturnType);
            SBQLNonAlgebraicExpression dotExpr = generateSBQLExpressionForLinqSelect(lambda, sBQLExpression[0], sBQLExpression[1]);            
            unaryExpr.Operand = dotExpr;

            return unaryExpr;
        }

        private SBQLExpression generateSBQLExpressionForLinqAlgebraicOperator(LINQOperator linqOper, MethodInfo methodInfo, List<SBQLExpression> operands)
        {
            if (operands.Count != linqOper.Method.GetParameters().Count())
                throw new NotImplementedException();
            SBQLExpression result = null;


            SBQLBinaryAlgebraicExpression.BinaryExpressionType binaryType;
            SBQLUnaryAlgebraicExpression.UnaryExpressionType unaryType;

            if (LINQOperators.linqOperator2sbqlBinary.ContainsKey(linqOper.Type))
            {
                binaryType = LINQOperators.linqOperator2sbqlBinary[linqOper.Type];

                SBQLBinaryAlgebraicExpression bexpr = SBQLExpression.BinaryAlgebraicExpression(binaryType, methodInfo.ReturnType);
                bexpr.LeftOperand = operands[0];
                bexpr.RightOperand = operands[1];
                bexpr.Method = methodInfo;
                result = bexpr;
            }
            else if (LINQOperators.linqOperator2sbqlUnary.ContainsKey(linqOper.Type))
            {
                unaryType = LINQOperators.linqOperator2sbqlUnary[linqOper.Type];

                SBQLUnaryAlgebraicExpression uexpr = SBQLExpression.UnaryAlgebraicExpression(unaryType, methodInfo.ReturnType);
                uexpr.Operand = operands[0];
                uexpr.Method = methodInfo;
                result = uexpr;
            }
            else
            {
                throw new NotImplementedException("generateSBQLExpressionForLinqAlgebraicOperator - unknown LINQOperator " + linqOper.Name);
            }

            return result;
        }
        
    }

}
