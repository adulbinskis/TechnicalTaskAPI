using TechnicalTaskAPI.ORM.Entities;

namespace TechnicalTaskAPI.ORM.Entities.Interfaces
{
    public interface IAuditable
    {
        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedById { get; set; }
        public ApplicationUser CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string UpdatedById { get; set; }
        public ApplicationUser UpdatedBy { get; set; }
    }
}
