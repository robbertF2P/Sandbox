using System.Linq.Expressions;
using System.Reflection;

namespace PrimaveraExcelReader.Mapping;

internal static class ExpressionBindingHelper
{
    public static Action<T, string?> CreateSetter<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression)
    {
        Action<T, TProperty> assign = CompileSetter(propertyExpression);
        string propertyName = GetPropertyName(propertyExpression);

        return (target, raw) =>
        {
            TProperty parsed = ExcelCellParser.Parse<TProperty>(raw, propertyName);
            assign(target, parsed);
        };
    }

    public static string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression)
    {
        if (propertyExpression.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Expression must be a property access.", nameof(propertyExpression));
        }

        if (memberExpression.Member is not PropertyInfo)
        {
            throw new ArgumentException("Expression must reference a property.", nameof(propertyExpression));
        }

        return memberExpression.Member.Name;
    }

    private static Action<T, TProperty> CompileSetter<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression)
    {
        if (propertyExpression.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Expression must be a property access.", nameof(propertyExpression));
        }

        if (memberExpression.Member is not PropertyInfo propertyInfo)
        {
            throw new ArgumentException("Expression must reference a property.", nameof(propertyExpression));
        }

        if (!propertyInfo.CanWrite)
        {
            throw new ArgumentException($"Property '{propertyInfo.Name}' is read-only.", nameof(propertyExpression));
        }

        ParameterExpression targetParameter = Expression.Parameter(typeof(T), "target");
        ParameterExpression valueParameter = Expression.Parameter(typeof(TProperty), "value");

        MemberExpression propertyAccess = Expression.Property(targetParameter, propertyInfo);
        BinaryExpression assign = Expression.Assign(propertyAccess, valueParameter);

        return Expression.Lambda<Action<T, TProperty>>(assign, targetParameter, valueParameter).Compile();
    }
}
