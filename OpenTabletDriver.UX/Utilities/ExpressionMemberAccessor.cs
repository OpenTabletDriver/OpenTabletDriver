using System.Linq.Expressions;

namespace OpenTabletDriver.UX.Utilities
{
    public class ExpressionMemberAccessor : ExpressionVisitor
    {
        private readonly Stack<MemberExpression> _expressions = new Stack<MemberExpression>();

        /// <summary>
        /// Accesses a member in an expression from another expression.
        /// </summary>
        /// <param name="parent">The source expression in which to access the child member.</param>
        /// <param name="child">The child member to access.</param>
        public Expression AccessMember(Expression parent, Expression child)
        {
            Visit(child);

            var final = parent;
            while (_expressions.TryPop(out var next))
                final = Expression.MakeMemberAccess(final, next.Member);

            return final;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _expressions.Push(node);
            return base.VisitMember(node);
        }
    }
}
