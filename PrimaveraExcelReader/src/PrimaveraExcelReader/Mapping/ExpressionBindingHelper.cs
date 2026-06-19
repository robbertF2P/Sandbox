using System.Linq.Expressions;
using System.Reflection;

namespace PrimaveraExcelReader.Mapping;

internal static class ExpressionBindingHelper
{
    public static Action<T, string?> CreateStringSetter<T>(Expression<Func<T, string>> propertyExpression)
    {
        Action<T, string> baseSetter = CompileSetter<T, string>(propertyExpression);
        return (target, value) => baseSetter(target, value ?? string.Empty);
    }

    public static Action<T, string?> CreateNullableStringSetter<T>(Expression<Func<T, string?>> propertyExpression)
    {
        return CompileSetter<T, string?>(propertyExpression);
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
