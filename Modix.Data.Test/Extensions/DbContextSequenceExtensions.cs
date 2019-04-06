using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore
{
    public static class DbContextSequenceExtensions
    {
        public static void InitializeSequenceSequenceToValue<TContext, TEntity, TProperty>(
            this TContext context,
            Expression<Func<TContext, DbSet<TEntity>>> tableSelector,
            Expression<Func<TEntity, TProperty>> propertySelector)
                where TContext : DbContext
                where TEntity : class
        {
            var valueGenerator = GetGetResettableSequenceValueGenerator(context, tableSelector, propertySelector);

            var table = tableSelector.Compile().Invoke(context).Local;

            valueGenerator.SetValue(default(TProperty));
        }

        public static void ResetSequenceToValue<TContext, TEntity, TProperty>(
            this TContext context,
            Expression<Func<TContext, DbSet<TEntity>>> tableSelector,
            Expression<Func<TEntity, TProperty>> propertySelector,
            TProperty value)
                where TContext : DbContext
                where TEntity : class
        {
            var valueGenerator = GetGetResettableSequenceValueGenerator(context, tableSelector, propertySelector);

            var table = tableSelector.Compile().Invoke(context).Local;

            valueGenerator.SetValue(value);
        }

        public static void ResetSequenceToMaxValue<TContext, TEntity, TProperty>(
            this TContext context,
            Expression<Func<TContext, DbSet<TEntity>>> tableSelector,
            Expression<Func<TEntity, TProperty>> propertySelector)
                where TContext : DbContext
                where TEntity : class
        {
            var valueGenerator = GetGetResettableSequenceValueGenerator(context, tableSelector, propertySelector);

            var table = tableSelector.Compile().Invoke(context).Local;

            valueGenerator.SetValue(table.Any()
                ? table.Max(propertySelector.Compile())
                : default(TProperty));
        }

        private static ResettableSequenceValueGenerator<TProperty> GetGetResettableSequenceValueGenerator<TContext, TEntity, TProperty>(
            this TContext context,
            Expression<Func<TContext, DbSet<TEntity>>> tableSelector,
            Expression<Func<TEntity, TProperty>> propertySelector)
                where TContext : DbContext
                where TEntity : class
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (tableSelector is null)
                throw new ArgumentNullException(nameof(tableSelector));

            if (propertySelector is null)
                throw new ArgumentNullException(nameof(propertySelector));

            var entity = context.Model.FindEntityType(typeof(TEntity).FullName);

            if (entity.IsQueryType)
                throw new ArgumentException($"{entity.Name} is a query record, not a table record.", nameof(tableSelector));

            if (!(propertySelector.Body is MemberExpression memberSelector) || (memberSelector.Member.MemberType != MemberTypes.Property))
                throw new ArgumentException($"Expression {propertySelector.Body.ToString()} does not select a property of record type {entity.Name}", nameof(propertySelector));

            var property = entity.FindProperty(memberSelector.Member.Name);

            if (property.ValueGenerated == ValueGenerated.Never)
                throw new ArgumentException($"Property {property.Name} of entity {entity.Name} is not configured for value generation");

            if (!_valueGeneratorConstructorsByValueType.TryGetValue(property.ClrType, out var valueGeneratorConstructor))
                throw new ArgumentException($"Unable to generate values of type {property.ClrType.Name}. No suitable {nameof(ValueGenerator)} exists");

            return context.GetService<IValueGeneratorCache>()
                .GetOrAdd(property, entity, valueGeneratorConstructor)
                as ResettableSequenceValueGenerator<TProperty>;
        }

        private static Dictionary<Type, Func<IProperty, IEntityType, ValueGenerator>> _valueGeneratorConstructorsByValueType
            = new Dictionary<Type, Func<IProperty, IEntityType, ValueGenerator>>()
            {
                [typeof(long)] = (p, e) => new ResettableInt64SequenceValueGenerator()
            };        
    }
}
