using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Order.Domain.Common;

namespace Order.Infrastructure.Data.Interceptors
{
    public class AuditableEntityInterceptor(TimeProvider timeProvider)
    : SaveChangesInterceptor
    {
        private readonly TimeProvider _timeProvider = timeProvider;

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            UpdateEntities(eventData.Context);

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            UpdateEntities(eventData.Context);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateEntities(DbContext? context)
        {
            if (context is null)
                return;

            var utcNow = _timeProvider.GetUtcNow();

            foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State is EntityState.Added or EntityState.Modified || entry.HasChangedOwnedEntities())
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.Entity.CreatedAtUtc = utcNow;
                    }

                    entry.Entity.LastModifiedUtc = utcNow;
                }
            }
        }
    }

    public static class EntityEntryExtensions
    {
        public static bool HasChangedOwnedEntities(this EntityEntry entry)
        {
            return entry.References.Any(r =>
                r.TargetEntry?.Metadata.IsOwned() == true &&
                (r.TargetEntry.State == EntityState.Added ||
                 r.TargetEntry.State == EntityState.Modified));
        }
    }
}
