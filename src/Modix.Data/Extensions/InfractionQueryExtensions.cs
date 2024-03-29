using System;
using System.Linq;
using System.Linq.Expressions;
using Modix.Data.Models.Moderation;

namespace Modix.Data.Extensions
{
    public static class InfractionQueryExtensions
    {
        public static Expression<Func<InfractionEntity, bool>> IsActive()
        {
            return x => x.DeleteActionId == null;
        }

        public static IQueryable<InfractionEntity> WhereIsActive(this IQueryable<InfractionEntity> source)
        {
            return source.Where(IsActive());
        }
    }
}
