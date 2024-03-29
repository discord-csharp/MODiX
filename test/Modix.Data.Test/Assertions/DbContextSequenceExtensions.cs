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
                where TProperty : struct
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
                : default(TProperty)!);
        }

        private static ResettableSequenceValueGenerator<TProperty> GetGetResettableSequenceValueGenerator<TContext, TEntity, TProperty>(
            this TContext context,
            Expression<Func<TContext, DbSet<TEntity>>> tableSelector,
            Expression<Func<TEntity, TProperty>> propertySelector)
                where TContext : DbContext
                where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(tableSelector);
            ArgumentNullException.ThrowIfNull(propertySelector);

            var entity = context.Model.FindEntityType(typeof(TEntity).FullName!)
                ?? throw new ArgumentException($"Could not find entity of type {typeof(TEntity).FullName}");

            if (propertySelector.Body is not MemberExpression memberSelector || (memberSelector.Member.MemberType != MemberTypes.Property))
                throw new ArgumentException($"Expression {propertySelector.Body} does not select a property of record type {entity.Name}", nameof(propertySelector));

            var property = entity.FindProperty(memberSelector.Member.Name)!;

            if (property.ValueGenerated == ValueGenerated.Never)
                throw new ArgumentException($"Property {property.Name} of entity {entity.Name} is not configured for value generation");

            if (!_valueGeneratorConstructorsByValueType.TryGetValue(property.ClrType, out var valueGeneratorConstructor))
                throw new ArgumentException($"Unable to generate values of type {property.ClrType.Name}. No suitable {nameof(ValueGenerator)} exists");

            return (ResettableSequenceValueGenerator<TProperty>)context.GetService<IValueGeneratorCache>()
                                                                       .GetOrAdd(property, entity, valueGeneratorConstructor);
        }

        private static readonly Dictionary<Type, Func<IProperty, ITypeBase, ValueGenerator>> _valueGeneratorConstructorsByValueType
            = new Dictionary<Type, Func<IProperty, ITypeBase, ValueGenerator>>()
            {
                [typeof(long)] = (p, e) => new ResettableInt64SequenceValueGenerator()
            };
    }
}
