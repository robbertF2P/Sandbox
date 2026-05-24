using System;
using System.Linq.Expressions;

namespace ContextTests.Dal.Query
{
    abstract class TypedPredicateBuilder<TModel>
    {
        internal Expression<Func<TModel, bool>> Predicate;
        protected TypedPredicateBuilder<TModel> RightPredicateBuilder;
    }
}