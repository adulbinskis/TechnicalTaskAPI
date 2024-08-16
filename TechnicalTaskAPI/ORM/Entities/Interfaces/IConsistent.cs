namespace TechnicalTaskAPI.ORM.Entities.Interfaces
{
    public interface IConsistent
    {
        public byte[] RowVersion { get; set; }
    }
}
