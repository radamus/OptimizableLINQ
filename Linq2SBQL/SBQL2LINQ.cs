using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;


namespace LINQ_SBQLConverters
{
    using SBQL.Expressions;
    using SBQL.Environment;
    using TypeHelpers;


    class SBQL2Linq : SBQLExpressionVisitor
    {
        ENVS4Linq environmentStack = new ENVS4Linq();
        protected internal override object VisitAuxNameExpression(SBQLAuxNameExpression literal)
        {
            throw new WrongSBQLTree("aux name should not be visited alone");
        }

        public Expression Convert(SBQLExpression expr)
        {

            Expression result = this.Visit(expr) as Expression;
            return result;
        }


        public LambdaExpression Convert(SBQLLambdaExpression query)
        {
            List<ParameterExpression> lambdaParameterExpressions = new List<ParameterExpression>();
            List<Type> genericParameters = new List<Type>();
            
            if (query.Lambda == null)
                throw new WrongSBQLTree("SBQL lambda expression must have Lambda != null");
            this.environmentStack.CreateEnv();
            foreach (LambdaInfo.LambdaParameter b in query.Lambda.parameters)
            {
                ParameterExpression pe = Expression.Parameter(b.Type, b.Name);
                lambdaParameterExpressions.Add(pe);
                genericParameters.Add(b.Type);
                this.environmentStack.PushBinder(ParameterExpressionBinder.CreateBinder(pe));
            }
            genericParameters.Add(query.Lambda.ResultType);
            Type lambdaType = TypeExtensions.paramNumber2delegateType[query.Lambda.parameters.Count];
            Type lambdaGenericType = lambdaType.MakeGenericType(genericParameters.ToArray());
            Expression result = this.Visit(query.Body) as Expression;
                     
            
            LambdaExpression le = Expression.Lambda( result, lambdaParameterExpressions);
            this.environmentStack.DestroyEnv();
            return le;
        }

        protected internal override object VisitCreateExpression(SBQLCreateExptession expr)
        {
            throw new NotImplementedException();
        }
        protected internal override object VisitBinaryAlgebraicExpression(SBQLBinaryAlgebraicExpression expr)
        {

            ExpressionType exprType;
            Expression result = null;
            if (LINQOperators.sbqlBinary2linqExpressionType.ContainsKey(expr.OperatorType))
            {
                exprType = LINQOperators.sbqlBinary2linqExpressionType[expr.OperatorType];
                Expression lexpr = visitOperand(expr.LeftOperand);

                Expression rexpr = visitOperand(expr.RightOperand);
                if (expr.Method != null)
                {
                    result = Expression.MakeBinary(exprType, lexpr, rexpr, false, expr.Method);
                }
                else
                {
                    result = Expression.MakeBinary(exprType, lexpr, rexpr);
                }

                return result;
            }
            else
            {
                return convertBinaryOperator2ExtensionMethodCall(expr);
            }



        }



        protected internal override object VisitLiteralExpression<T>(SBQLLiteralExpression<T> literal)
        {
            Expression result = Expression.Constant(literal.Value);
            return result;
        }

        protected internal override object VisitNameExpression(SBQLNameExpression name)
        {
            //name expression can represent MemberAccess or AUxName or Local variable (member access) but it should be generated from dot expression
            if (name.NameSource != SBQLNameExpression.NameType.Param)
                throw new WrongSBQLTree("visit name expression only for creation of parameter expression ");
            if (name.ResultType == null)
                throw new WrongSBQLTree("unable to create parameter expression without a type");
            ParameterExpression peb = this.environmentStack.BindParameter(name.Name);

            //name expression is supported in the context of paramExpression
            //            Expression expr = Expression.Parameter(name.ResultType, name.Name);

            return peb;
        }

        protected internal override object VisitNonAlgebraicExpression(SBQLNonAlgebraicExpression expr)
        {
            Expression result;

            switch (expr.OperatorType)
            {
                case SBQLNonAlgebraicExpression.NonAlgebraicOperator.DOT:
                    result = convertNavigation2Linq(expr);
                    break;
                case SBQLNonAlgebraicExpression.NonAlgebraicOperator.WHERE:
                    result = convertWhere2Linq(expr);
                    break;
                default:
                    throw new NotImplementedException("unimplemented non algebraic operator");
            }


            return result;
        }

        private Expression convertWhere2Linq(SBQLNonAlgebraicExpression expr)
        {
            throw new NotImplementedException();
        }

        private Expression convertNavigation2Linq(SBQLNonAlgebraicExpression expr)
        {
            Expression result;
            if (isJoinExtensionPattern(expr))
                result = convert2LinqJoin(expr);
            else if (isQuantifierExtensionMethodPattern(expr))
                result = convert2LinqQuantifier(expr);
            else if (isWhereExtensionMethodPattern(expr))
                result = convert2LinqWhere(expr);
            else if (isSelectExtensionPattern(expr))
                result = convert2LinqSelect(expr);
            else if (isInstanceMethodCallPattern(expr))
                result = convert2LinqMethodCall(expr);
            else if (isMemberAccessPattern(expr) || isLocalVariablePattern(expr))
            {
                result = convert2LinqMemberAccess(expr);
            }            
            else
                throw new WrongSBQLTree("unable to find any conversion pattern for " + expr);
            return result;
        }

        private Expression convert2LinqJoin(SBQLNonAlgebraicExpression expr)
        {
            throw new NotImplementedException();
        }

        private bool isJoinExtensionPattern(SBQLNonAlgebraicExpression expr)
        {
            //if(expr.OperatorType == SBQLNonAlgebraicExpression.NonAlgebraicOperator.DOT
            //    && expr.LeftOperand is SBQLNonAlgebraicExpression
            //    && (expr.LeftOperand as SBQLNonAlgebraicExpression).OperatorType == SBQLNonAlgebraicExpression.NonAlgebraicOperator.DOT
            //    && (expr.LeftOperand as SBQLNonAlgebraicExpression).RightOperand is SBQLNonAlgebraicExpression
            //    && (expr.LeftOperand as SBQLNonAlgebraicExpression).LeftOperand is SBQLNonAlgebraicExpression
            //    && (expr.LeftOperand as SBQLNonAlgebraicExpression).RightOperand is SBQLNonAlgebraicExpression
            //    $$ ((expr.LeftOperand as SBQLNonAlgebraicExpression).LeftOperand as SBQLNonAlgebraicExpression))
            //    return true;
            return false;
        }


        
        protected internal override object VisitUnaryAlgebraicExpression(SBQLUnaryAlgebraicExpression expr)
        {

            ExpressionType exprType;
            switch (expr.OperatorType)
            {

                case SBQLUnaryAlgebraicExpression.UnaryExpressionType.Negate:
                    exprType = ExpressionType.Negate;
                    break;
                default:
                    return convertUnaryOperator2ExtensionMethodCall(expr);


            }
            Expression operand = visitOperand(expr.Operand);
            Expression result;
            if (expr.Method != null)
            {
                //TODO convert requires type parameter
                result = Expression.MakeUnary(exprType, operand, null, expr.Method);
            }
            else
            {
                //TODO convert requires type parameter
                result = Expression.MakeUnary(exprType, operand, null);
            }


            return result;

        }

        protected internal override object VisitProcedureCallExpression(SBQLProcedureCallExpression expr)
        {
            throw new NotImplementedException();
        }

        protected internal override object VisitWrapperExpression<T>(SBQLWrapperExpression<T> expr)
        {
            if (expr.Expression is Expression)
            {
                return expr.Expression;

            }
            else
            {
                throw new NotSupportedException("not a Linq expression");
            }
        }




        private Expression visitOperand(SBQLExpression expr)
        {

            object o = this.Visit(expr);
            if (o is Expression)
                return o as Expression;
            else
                throw new InvalidProgramException("visitors should return Exception type");
        }

        public bool isWhereExtensionMethodPattern(SBQLNonAlgebraicExpression expr)
        {
            if (expr.OperatorType == SBQLNonAlgebraicExpression.NonAlgebraicOperator.DOT
                && expr.LeftOperand is SBQLNonAlgebraicExpression
                && expr.RightOperand is SBQLNameExpression
                && (expr.LeftOperand as SBQLNonAlgebraicExpression).OperatorType == SBQLNonAlgebraicExpression.NonAlgebraicOperator.WHERE
                && (expr.LeftOperand as SBQLNonAlgebraicExpression).LeftOperand is SBQLAuxNameExpression
                && ((expr.LeftOperand as SBQLNonAlgebraicExpression).LeftOperand as SBQLAuxNameExpression).Name == (expr.RightOperand as SBQLNameExpression).Name)
            {
                return true;
            }
            return false;
        }

        public bool isSelectExtensionPattern(SBQLNonAlgebraicExpression expr)
        {
            if (expr.OperatorType == SBQLNonAlgebraicExpression.NonAlgebraicOperator.DOT
                && expr.LeftOperand is SBQLAuxNameExpression)
                return true;
            return false;
        }

        public bool isInstanceMethodCallPattern(SBQLNonAlgebraicExpression expr)
        {
            if (expr.OperatorType == SBQLNonAlgebraicExpression.NonAlgebraicOperator.DOT
                && expr.RightOperand is SBQLProcedureCallExpression)
                return true;
            return false;
        }

        public bool isMemberAccessPattern(SBQLNonAlgebraicExpression expr)
        {
            if (expr.OperatorType == SBQLNonAlgebraicExpression.NonAlgebraicOperator.DOT
                && expr.RightOperand is SBQLNameExpression
                && (expr.RightOperand as SBQLNameExpression).NameSource == SBQLNameExpression.NameType.Member)
                return true;
            return false;
        }

        public bool isLocalVariablePattern(SBQLNonAlgebraicExpression expr)
        {
            if (expr.OperatorType == SBQLNonAlgebraicExpression.NonAlgebraicOperator.DOT
                && expr.RightOperand is SBQLLinqExpressionWrapper
                && (expr.RightOperand as SBQLNameExpression).NameSource == SBQLNameExpression.NameType.Local)
                return true;
            return false;
        }

        private bool isQuantifierExtensionMethodPattern(SBQLNonAlgebraicExpression expr)
        {
            return (expr.OperatorType == SBQLNonAlgebraicExpression.NonAlgebraicOperator.FORSOME || expr.OperatorType == SBQLNonAlgebraicExpression.NonAlgebraicOperator.FORALL)
                && expr.LeftOperand is SBQLAuxNameExpression;
        }

        private Expression convert2LinqQuantifier(SBQLNonAlgebraicExpression expr)
        {
            //forall (source as selem) predicateBody
            //source.All(selem => predicateBody)

            SBQLAuxNameExpression selemNameExpr = expr.LeftOperand as SBQLAuxNameExpression;
            SBQLExpression predicateBodyExpr = expr.RightOperand;
            SBQLExpression sourceExpr = selemNameExpr.Operand;


            String operatorName = expr.OperatorType == SBQLNonAlgebraicExpression.NonAlgebraicOperator.FORSOME ? "Any" : "All";

            //LambdaExpression le = Expression.Lambda(linqPredicateBodyExpr, Expression.Parameter(selemNameExpr.ResultType, selemNameExpr.Name));
            //UnaryExpression quote = Expression.MakeUnary(ExpressionType.Quote, le, le.GetType());

            ////TODO refactor 
            //MethodInfo quantifierMethod = typeof(Queryable).GetGenericMethod(expr.OperatorType == SBQLNonAlgebraicExpression.NonAlgebraicOperator.FORSOME? "Any" : "All", new Type[] { typeof(IQueryable<>), typeof(Expression<>) });
            //quantifierMethod = quantifierMethod.MakeGenericMethod(selemNameExpr.ResultType, expr.ResultType);
            //MethodCallExpression quantifierCall = Expression.Call(quantifierMethod, linqSourceExpr, quote);
            //return quantifierCall;

            return generateMethodCallExpressionForLambdaOperator(sourceExpr, predicateBodyExpr, new LambdaInfo.LambdaParameter[] { new LambdaInfo.LambdaParameter { Type = selemNameExpr.ResultType, Name = selemNameExpr.Name } }, operatorName, new Type[] { selemNameExpr.ResultType });
        }

        public MethodCallExpression generateMethodCallExpressionForLambdaOperator(SBQLExpression sbqlSource, SBQLExpression sbqlLambdaBody, LambdaInfo.LambdaParameter[] lambdaParams, String methodName, Type[] genericParameterTypes)
        {


            List<ParameterExpression> lambdaParameterExpressions = new List<ParameterExpression>();
            Expression source = visitOperand(sbqlSource);

            this.environmentStack.CreateEnv();
            foreach (LambdaInfo.LambdaParameter lp in lambdaParams)
            {
                ParameterExpression pe = Expression.Parameter(lp.Type, lp.Name);
                this.environmentStack.PushBinder(ParameterExpressionBinder.CreateBinder(pe));
                lambdaParameterExpressions.Add(pe);
            }
            //where the hell is nested(source)? ;)

            Expression lambdaBody = visitOperand(sbqlLambdaBody);

            this.environmentStack.DestroyEnv();

            LambdaExpression le = Expression.Lambda(lambdaBody, lambdaParameterExpressions.ToArray());
            UnaryExpression quote = Expression.MakeUnary(ExpressionType.Quote, le, le.GetType());

            MethodInfo method = typeof(Queryable).GetGenericMethod(methodName, new Type[] { typeof(IQueryable<>), typeof(Expression<>) });
            method = method.MakeGenericMethod(genericParameterTypes);
            MethodCallExpression methodCall = Expression.Call(method, source, quote);
            return methodCall;
        }
        public Expression convert2LinqWhere(SBQLNonAlgebraicExpression expr)
        {
            //((source as selem) where predicateBody).selem
            //source.Where(selem => predicateBody)

            SBQLNonAlgebraicExpression whereExpr = expr.LeftOperand as SBQLNonAlgebraicExpression;

            // SBQLNameExpression bindSelemNameExpr = expr.RightOperand as SBQLNameExpression;

            SBQLAuxNameExpression selemNameExpr = whereExpr.LeftOperand as SBQLAuxNameExpression;

            SBQLExpression predicateBodyExpr = whereExpr.RightOperand;
            SBQLExpression sourceExpr = selemNameExpr.Operand;

            return generateMethodCallExpressionForLambdaOperator(sourceExpr, predicateBodyExpr, new LambdaInfo.LambdaParameter[] { new LambdaInfo.LambdaParameter { Type = selemNameExpr.ResultType, Name = selemNameExpr.Name } }, "Where", new Type[] { selemNameExpr.ResultType });
            //LambdaExpression le = Expression.Lambda(linqPredicateBodyExpr, Expression.Parameter(selemNameExpr.ResultType, selemNameExpr.Name));
            //UnaryExpression quote = Expression.MakeUnary(ExpressionType.Quote, le, le.GetType());
            //MethodInfo whereGenericMethod = typeof(Queryable).GetGenericMethod("Where", new Type[] { typeof(IQueryable<>), typeof(Expression<>) });
            //MethodInfo whereMethod = whereGenericMethod.MakeGenericMethod(selemNameExpr.ResultType);
            //MethodCallExpression whereCall = Expression.Call(whereMethod, linqSourceExpr, quote);
            //return whereCall;

        }

        public Expression convert2LinqSelect(SBQLNonAlgebraicExpression expr)
        {
            //((source as selem).selectorBody)
            // source.Select(selem => selectorBody)



            SBQLAuxNameExpression selemNameExpr = expr.LeftOperand as SBQLAuxNameExpression;
            if (!selemNameExpr.IsParameterRepresentation)
                throw new WrongSBQLTree("conversion to linq select - AuxnameExpression does not represents a parameter name ");
            SBQLExpression sourceExpr = selemNameExpr.Operand;
            SBQLExpression selectorBodyExpr = expr.RightOperand;

            return generateMethodCallExpressionForLambdaOperator(sourceExpr, selectorBodyExpr, new LambdaInfo.LambdaParameter[] { new LambdaInfo.LambdaParameter { Type = selemNameExpr.ResultType, Name = selemNameExpr.Name } }, "Select", new Type[] { selemNameExpr.ResultType, expr.ResultType });


            //LambdaExpression le = Expression.Lambda(linqSelectorBodyExpr, Expression.Parameter(selemNameExpr.ResultType, selemNameExpr.Name));
            //UnaryExpression quote = Expression.MakeUnary(ExpressionType.Quote, le, le.GetType());
            ////TODO refactor 
            //MethodInfo selectMethod = typeof(Queryable).GetGenericMethod("Select", new Type[] { typeof(IQueryable<>), typeof(Expression<>) });
            //selectMethod = selectMethod.MakeGenericMethod(selemNameExpr.ResultType, expr.ResultType);
            //MethodCallExpression selectCall = Expression.Call(selectMethod, linqSourceExpr, quote);
            //return selectCall;
        }

        public Expression convert2LinqMethodCall(SBQLNonAlgebraicExpression expr)
        {
            Expression instance = visitOperand(expr.LeftOperand);

            return Expression.Call(instance, (expr.RightOperand as SBQLProcedureCallExpression).MethodInfo);
        }

        public Expression convert2LinqMemberAccess(SBQLNonAlgebraicExpression expr)
        {
            Expression left = visitOperand(expr.LeftOperand);
            //TODO Type declaringType; obtain field type by name
            // if(left is ParameterExpression) else can be constant 
            if ((expr.RightOperand as SBQLNameExpression).Member != null)
            {
                MemberExpression mexpr = Expression.MakeMemberAccess(left, (expr.RightOperand as SBQLNameExpression).Member);
                return mexpr;
            }

            else throw new WrongSBQLTree("unsupported conversion 2 memeber access - no memberinfo for: " + expr.RightOperand);


        }

        public Expression convertBinaryOperator2ExtensionMethodCall(SBQLBinaryAlgebraicExpression expr)
        {
            Expression result = null;
            Expression lexpr = visitOperand(expr.LeftOperand);
            Expression rexpr = visitOperand(expr.RightOperand);
            result = Expression.Call(expr.Method, lexpr, rexpr);

            return result;
        }

        public Expression convertUnaryOperator2ExtensionMethodCall(SBQLUnaryAlgebraicExpression expr)
        {
            Expression result = null;
            Expression oexpr = visitOperand(expr.Operand);

            result = Expression.Call(expr.Method, oexpr);

            return result;
        }
    }


}
