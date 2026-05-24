using ContextTests.Contracts;

namespace ContextTests.Dal.Query
{
    class StudentQueryBuilder : TypedPredicateBuilder<Model.Student>, IQueryBuilder, IQueryComposition
    {
        public IQueryOperator<T> Field<T>(string member)
        {
            switch (member.ToLower())
            {
                case "enrolledon":
                    return (IQueryOperator<T>) new DataTimeQueryOperator<Model.Student>(this, m => m.EnrolledOn);
            }
            return null;
        }

        public IQueryBuilder And()
        {
            if (Predicate == null) Predicate = RightPredicateBuilder.Predicate;
            var right = RightPredicateBuilder.Predicate;
            Predicate = Predicate.And(right);
            return this;
        }

        public IQueryBuilder Or()
        {
            var right = RightPredicateBuilder.Predicate;
            Predicate = Predicate.Or(right);
            return this;
        }

    }
}
