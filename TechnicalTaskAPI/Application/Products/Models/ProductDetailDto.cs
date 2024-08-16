namespace TechnicalTaskAPI.Application.Products.Models
{
    public class ProductDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal PriceWithVAT { get; set; }
    }
}
