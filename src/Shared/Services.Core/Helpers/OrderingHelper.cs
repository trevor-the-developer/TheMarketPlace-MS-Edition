using System.Linq.Expressions;
using Services.Core.Entities;

namespace Services.Core.Helpers;

public static class OrderingHelper
{
    private static readonly Dictionary<string, string> PropertyMappings = new()
    {
        { "CompanyName", "Company.Name" }
        // Add more mappings as needed
    };
    
    public static IQueryable<TEntity> ApplyOrdering<TEntity>(
        IQueryable<TEntity> query, 
        string? orderBy, 
        bool? sortDescending = false)
    {
        if (string.IsNullOrEmpty(orderBy))
        {
            orderBy = nameof(BaseEntity.Id);
        }

        // Check if we have a mapped property name
        if (PropertyMappings.ContainsKey(orderBy))
        {
            orderBy = PropertyMappings[orderBy];
        }

        var parameter = Expression.Parameter(typeof(TEntity), "x");
        Expression property = parameter;

        // Split the property name in case it includes nested properties
        foreach (var prop in orderBy.Split('.'))
        {
            property = Expression.Property(property, prop);
        }

        var lambda = Expression.Lambda(property, parameter);
        var methodName = sortDescending == true ? "OrderByDescending" : "OrderBy";
        var resultExpression = Expression.Call(
            typeof(Queryable), 
            methodName, 
            new Type[] { query.ElementType, property.Type },
            query.Expression, 
            Expression.Quote(lambda)
        );

        return query.Provider.CreateQuery<TEntity>(resultExpression);
    }
}