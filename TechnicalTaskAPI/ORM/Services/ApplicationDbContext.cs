using System.Text.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechnicalTaskAPI.ORM.Entities;
using TechnicalTaskAPI.Application.Services.Interfaces;
using TechnicalTaskAPI.Application.Services;
using TechnicalTaskAPI.ORM.Entities.Interfaces;
using System.Security.Claims;
using TechnicalTaskAPI.ORM.Configurations;

// dotnet ef migrations add AddedQandATables -c ApplicationDbContext -o ORM\Migrations
// dotnet ef database update -c ApplicationDbContext

namespace TechnicalTaskAPI.ORM.Services
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Product> Products { get; set; }

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IDateTimeService dateTimeService,
            IHttpContextAccessor httpContextAccessor
            ) : base(options)
        {
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SyncRowVersion();
            AddAuditInfo();
            CreateAuditLogs();
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.ApplyConfiguration(new ProductConfiguration());
        }

        private void SyncRowVersion()
        {
            foreach (var entry in ChangeTracker.Entries<IConsistent>())
            {
                var propertyEntry = entry.Property(x => x.RowVersion);

                // Ensuring that the original value will come from the client - not from the database.
                propertyEntry.OriginalValue = propertyEntry.CurrentValue;
            }
        }

        private void AddAuditInfo()
        {
            var utcNow = _dateTimeService.UtcNow;

            ChangeTracker
                .Entries<IAuditable>()
                .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified)
                .AsParallel()
                .ForAll(x =>
                {
                    var entity = x.Entity;

                    if (x.State == EntityState.Added)
                    {
                        entity.CreatedAt = utcNow;
                        entity.CreatedById = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                    }

                    if (x.State == EntityState.Modified)
                    {
                        entity.UpdatedAt = utcNow;
                        entity.UpdatedById = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                    }
                });
        }

        private void CreateAuditLogs()
        {
            ChangeTracker.DetectChanges();
            var entries = ChangeTracker
                .Entries()
                .Where(x =>
                    x.Entity is not AuditLog &&
                    x.Entity is IAuditable &&
                    x.State != EntityState.Detached &&
                    x.State != EntityState.Unchanged)

                /// Preventing <see cref="InvalidOperationException"/> with message 'Collection was modified; enumeration operation may not execute.'
                .ToList();

            var utcNow = _dateTimeService.UtcNow;

            foreach (var entry in entries)
            {
                var auditLog = new AuditLog()
                {
                    EntityName = entry.Entity.GetType().Name,
                    CreatedAt = utcNow,
                    UserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                };

                var primaryKeys = new Dictionary<string, string>();
                var oldValues = new Dictionary<string, object>();
                var newValues = new Dictionary<string, object>();
                var affectedProperties = new List<string>();

                foreach (var property in entry.Properties)
                {
                    var propertyName = property.Metadata.Name;

                    if (property.Metadata.IsPrimaryKey())
                    {
                        primaryKeys[propertyName] = property.CurrentValue.ToString();
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditLog.Operation = AuditLogOperation.Create;
                            newValues[propertyName] = property.CurrentValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditLog.Operation = AuditLogOperation.Update;
                                oldValues[propertyName] = property.OriginalValue;
                                newValues.Add(propertyName, property.CurrentValue);
                                affectedProperties.Add(propertyName);
                            }

                            break;

                        case EntityState.Deleted:
                            auditLog.Operation = AuditLogOperation.Delete;
                            oldValues[propertyName] = property.OriginalValue;
                            break;
                    }
                }

                auditLog.EntityPrimaryKeys = primaryKeys.Any() ? JsonSerializer.Serialize(primaryKeys) : null;
                auditLog.OldValues = oldValues.Any() ? JsonSerializer.Serialize(oldValues) : null;
                auditLog.NewValues = newValues.Any() ? JsonSerializer.Serialize(newValues) : null;
                auditLog.AffectedProperties = affectedProperties.Any() ? JsonSerializer.Serialize(affectedProperties) : null;

                AuditLogs.Add(auditLog);
            }
        }
    }
}
