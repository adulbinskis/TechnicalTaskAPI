namespace TechnicalTaskAPI.ORM.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
    }
}
