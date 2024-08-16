namespace TechnicalTaskAPI.Application.Products.Models
{
    public class PaginatedProductsDto
    {
        public int TotalPages { get; set; }
        public List<ProductDetailDto> Products { get; internal set; }
    }
}
