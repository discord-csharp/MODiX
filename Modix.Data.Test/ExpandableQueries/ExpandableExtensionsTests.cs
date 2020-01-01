using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Modix.Data.ExpandableQueries;

using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Modix.Data.Test.ExpandableQueries
{
    [TestFixture]
    public class ExpandableExtensionsTests
    {
        #region AsExpandable() Tests

        [Test]
        public void AsExpandable_QueryIsNull_ThrowsException()
        {
            Should.Throw<ArgumentNullException>(() =>
            {
                ExpandableExtensions.AsExpandable<object>(null!);
            });
        }

        [Test]
        public void AsExpandable_QueryIsExpandableQuery_ReturnsQuery()
        {
            var query = new ExpandableQueryProvider(Substitute.For<IQueryProvider>())
                .CreateQuery(Expression.Constant(null, typeof(object[])));

            query.AsQueryable().ShouldBeSameAs(query);
        }

        [Test]
        public void AsExpandable_Otherwise_ReturnsExpandableQuery()
            => Enumerable.Empty<object>()
                .AsQueryable()
                .AsExpandable()
                .ShouldBeOfType<ExpandableQuery<object>>();

        [TestCaseSource(nameof(ExpressionExpansionTestCases))]
        public void AsExpandable_Otherwise_TransformsExecutedExpression<TIn, TOut>(Expression<Func<TIn, TOut>> input, Expression<Func<TIn, TOut>> output)
        {
            var query = Substitute.For<IQueryable<TIn>>();
            var provider = Substitute.For<IQueryProvider>();
            query.Provider.Returns(provider);
            query.Expression.Returns(Expression.Constant(Enumerable.Empty<TIn>().AsQueryable()));

            query
                .AsExpandable()
                .Select(input)
                .ToArray();

            provider.ShouldHaveReceived(1)
                .Execute<IEnumerable<TOut>>(Arg.Is<MethodCallExpression>(x =>
                    (x != null)
                    && (x.Method.Name == nameof(Enumerable.Select))
                    && (x.Arguments.Count == 2)));

            var receivedExpression = (MethodCallExpression)provider.ReceivedCalls()
                .Where(x => x.GetMethodInfo().Name == nameof(IQueryProvider.Execute))
                .First()
                .GetArguments()
                .First();

            receivedExpression.Arguments[1].ShouldBe(output);
        }

        private static readonly TestCaseData[] ExpressionExpansionTestCases
            = {
                new TestCaseData(
                    (Expression<Func<object, object>>)(x => x),
                    (Expression<Func<object, object>>)(x => x))
                    .SetName("{m}(unity expression, without expansions)"),
                new TestCaseData(
                    (Expression<Func<object, string?>>)(x => x.ToString()),
                    (Expression<Func<object, string?>>)(x => x.ToString()))
                    .SetName("{m}(method call expression, without expansions)"),
                new TestCaseData(
                    (Expression<Func<IQueryable<object>, IQueryable<string>>>)(x => x.Select(y => y.ToString())!),
                    (Expression<Func<IQueryable<object>, IQueryable<string>>>)(x => x.Select(y => y.ToString())!))
                    .SetName("{m}(sub-expression, without expansions)"),
                new TestCaseData(
                    (Expression<Func<IQueryable<object>, IQueryable<string>>>)(x => x.Select(ObjectToStringExpansionExpression)),
                    (Expression<Func<IQueryable<object>, IQueryable<string>>>)(x => x.Select(y => y.ToString())!))
                    .SetName("{m}(sub-expression, with expansion)"),
                new TestCaseData(
                    (Expression<Func<IQueryable<object>, IQueryable<string>>>)(x => x.Select(ObjectToStringNonExpansionExpression)),
                    (Expression<Func<IQueryable<object>, IQueryable<string>>>)(x => x.Select(ObjectToStringNonExpansionExpression)))
                    .SetName("{m}(sub-expression, with non-expansion)"),
                new TestCaseData(
                    (Expression<Func<object, string>>)(x => x.Project(y => y.ToString())!),
                    (Expression<Func<object, string>>)(x => x.ToString()!))
                    .SetName("{m}(projection expression, without expansions)"),
                new TestCaseData(
                    (Expression<Func<object, string>>)(x => x.Project(ObjectToStringExpansionExpression)),
                    (Expression<Func<object, string>>)(x => x.ToString()!))
                    .SetName("{m}(projection expression, with expansions)"),
            };

        [ExpansionExpression]
        private static readonly Expression<Func<object, string>> ObjectToStringExpansionExpression
            = y => y.ToString()!;

        private static readonly Expression<Func<object, string>> ObjectToStringNonExpansionExpression
            = y => y.ToString()!;

        #endregion AsExpandable() Tests

        #region Project(projection) Tests

        [Test]
        public void Project_Always_ThrowsExcpetion()
        {
            Should.Throw<NotSupportedException>(() =>
            {
                new object().Project(x => x.ToString());
            });
        }

        #endregion Project(projection) Tests
    }
}
