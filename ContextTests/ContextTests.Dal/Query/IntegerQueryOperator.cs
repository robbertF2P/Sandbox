using System;
using System.Linq.Expressions;
using ContextTests.Contracts;

namespace ContextTests.Dal.Query
{
    class IntegerQueryOperator<TModel> : QueryOperator<TModel, int>, IQueryOperator<int>
    {
        public IntegerQueryOperator(
            TypedPredicateBuilder<TModel> queryComposer,
            Expression<Func<TModel, int>> expression)
            : base(queryComposer, expression)
        { }

        public IQueryComposition IsEqualTo(int value)
        {
            Func<int, bool> condition = val => val == value;
            return BuildAndStorePredicate(condition);
        }

        public IQueryComposition IsBetween(int @from, int to)
        {
            Func<int, bool> condition = val => val >= @from && val < to;
            return BuildAndStorePredicate(condition);
        }
    }
}