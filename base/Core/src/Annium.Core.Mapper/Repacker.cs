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
                return null!;

            return ex switch
            {
                BinaryExpression binary => Binary(binary)(source),
                ConstantExpression constant => constant,
                MethodCallExpression call => Call(call)(source),
                LambdaExpression lambda => Lambda(lambda)(source),
                MemberExpression member => Member(member)(source),
                MemberInitExpression memberInit => MemberInit(memberInit)(source),
                NewExpression construction => New(construction)(source),
                ParameterExpression param => source,
                UnaryExpression unary => Unary(unary)(source),
                _ => throw new InvalidOperationException($"Can't repack {ex.NodeType} expression"),
            };
        };

        private Func<Expression, Expression> Binary(BinaryExpression ex) => (Expression source) =>
            Expression.MakeBinary(
                ex.NodeType,
                Repack(ex.Left)(source),
                Repack(ex.Right)(source),
                ex.IsLiftedToNull,
                ex.Method,
                ex.Conversion
            );

        private Func<Expression, Expression> Call(MethodCallExpression ex) => (Expression source) =>
            Expression.Call(Repack(ex.Object)(source), ex.Method, ex.Arguments.Select(a => Repack(a)(source)).ToArray());

        private Func<Expression, Expression> Lambda(LambdaExpression ex) => (Expression source) =>
            Expression.Lambda(Repack(ex.Body)(source), new[] { source as ParameterExpression });

        private Func<Expression, Expression> Member(MemberExpression ex) => (Expression source) =>
            Expression.MakeMemberAccess(Repack(ex.Expression)(source), ex.Member);

        private Func<Expression, Expression> MemberInit(MemberInitExpression ex) => (Expression source) =>
            Expression.MemberInit(
                Repack(ex.NewExpression)(source) as NewExpression,
                ex.Bindings.Select(b =>
                {
                    if (b is MemberAssignment ma)
                        return ma.Update(Repack(ma.Expression)(source));

                    return b;
                })
            );

        private Func<Expression, Expression> New(NewExpression ex) => (Expression source) =>
            Expression.New(ex.Constructor, ex.Arguments.Select(a => Repack(a)(source)));

        private Func<Expression, Expression> Unary(UnaryExpression ex) => (Expression source) =>
            Expression.MakeUnary(ex.NodeType, Repack(ex.Operand)(source), ex.Type, ex.Method);
    }
}