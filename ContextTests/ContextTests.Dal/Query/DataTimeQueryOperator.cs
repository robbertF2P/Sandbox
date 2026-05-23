using System;
using System.Linq.Expressions;
using ContextTests.Contracts;

namespace ContextTests.Dal.Query
{
    class DataTimeQueryOperator<TModel> : QueryOperator<TModel, DateTime>, IQueryOperator<DateTime>
    {
        public DataTimeQueryOperator(
            TypedPredicateBuilder<TModel> queryComposer,
            Expression<Func<TModel, DateTime>> expression)
            : base(queryComposer, expression)
        { }

        public IQueryComposition IsEqualTo(DateTime value)
        {
            Func<DateTime, bool> condition = date => date == value;
            return BuildAndStorePredicate(condition);
        }

        public IQueryComposition IsBetween(DateTime @from, DateTime to)
        {
            Func<DateTime, bool> condition = date => date >= @from && date < to;
            return BuildAndStorePredicate(condition);
        }
    }
}