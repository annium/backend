using System;
using System.Linq;
using System.Linq.Expressions;

namespace Annium.Core.Mapper
{
    // Summary:
    //      Repacks given expression with given source expression, replacing parameter expressions to given source expression
    public class Repacker
    {
        public Func<Expression, Expression> Repack(Expression ex) => (Expression source) =>
        {
            if (ex == null)
                return null;

            switch (ex)
            {
                case ConstantExpression constant:
                    return constant;
                case LambdaExpression lambda:
                    return Lambda(lambda) (source);
                case MemberExpression member:
                    return Member(member) (source);
                case MemberInitExpression memberInit:
                    return MemberInit(memberInit) (source);
                case MethodCallExpression call:
                    return Call(call) (source);
                case NewExpression construction:
                    return New(construction) (source);
                case ParameterExpression param:
                    return source;
                default:
                    throw new InvalidOperationException($"Can't repack {ex.NodeType} expression");
            }
        };

        private Func<Expression, Expression> Lambda(LambdaExpression ex) => (Expression source) =>
            Expression.Lambda(Repack(ex.Body) (source), new [] { source as ParameterExpression });

        private Func<Expression, Expression> Member(MemberExpression ex) => (Expression source) =>
            Expression.MakeMemberAccess(Repack(ex.Expression) (source), ex.Member);

        private Func<Expression, Expression> MemberInit(MemberInitExpression ex) => (Expression source) =>
            Expression.MemberInit(
                Repack(ex.NewExpression) (source) as NewExpression,
                ex.Bindings.Select(b =>
                {
                    if (b is MemberAssignment ma)
                        return ma.Update(Repack(ma.Expression) (source));

                    return b;
                })
            );

        private Func<Expression, Expression> Call(MethodCallExpression ex) => (Expression source) =>
            Expression.Call(Repack(ex.Object) (source), ex.Method, ex.Arguments.Select(a => Repack(a) (source)).ToArray());

        private Func<Expression, Expression> New(NewExpression ex) => (Expression source) =>
            Expression.New(ex.Constructor, ex.Arguments.Select(a => Repack(a) (source)));
    }
}