namespace TechnicalTaskAPI.ORM.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }

        public string EntityPrimaryKeys { get; set; }
        public string EntityName { get; set; }
        public AuditLogOperation Operation { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public string AffectedProperties { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }

    public enum AuditLogOperation
    {
        Create = 0,
        Update = 1,
        Delete = 2
    }
}
