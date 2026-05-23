using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContextTests.Contracts
{
    public interface IQueryBuilder
    {
        IQueryOperator<T> Field<T>(string member);
    }

    public interface IQueryOperator<T>
    {
        IQueryComposition IsEqualTo(T value);
        IQueryComposition IsBetween(T from, T to);
    }

    public interface IQueryComposition
    {
        IQueryBuilder And();
        IQueryBuilder Or();


    }
}
