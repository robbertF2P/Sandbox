using System;
using System.Linq.Expressions;
using ContextTests.Contracts;
using LinqKit;

namespace ContextTests.Dal.Query
{
    abstract class QueryOperator<TModel, T> : TypedPredicateBuilder<TModel>
    {
        protected readonly TypedPredicateBuilder<TModel> QueryComposer;
        protected readonly Expression<Func<TModel, T>> PropertyExpression;

        protected QueryOperator(TypedPredicateBuilder<TModel> queryComposer, Expression<Func<TModel, T>> expression)
        {
            QueryComposer = queryComposer;
            PropertyExpression = expression;
        }

        protected IQueryComposition BuildAndStorePredicate(Func<T, bool> condition)
        {
            Expression<Func<TModel, bool>> pred = model => condition(PropertyExpression.Invoke(model));
            StorePredicate(pred);

            return (IQueryComposition) QueryComposer;
        }

        private void StorePredicate(Expression<Func<TModel, bool>> pred)
        {
            if (QueryComposer.Predicate == null)
            {
                QueryComposer.Predicate = pred;
            }
            else
            {
                Predicate = pred;
            }
        }
    }
}