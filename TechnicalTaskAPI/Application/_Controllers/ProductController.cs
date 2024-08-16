using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TechnicalTaskAPI.Application.Products.Commands;
using TechnicalTaskAPI.Application.Products.Models;
using TechnicalTaskAPI.Application.Products.Querirs;

namespace TechnicalTaskAPI.Application._Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : MediatRController
    {
        [HttpGet("[action]")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<ProductDetailDto>> GetProduct([FromQuery] GetProduct.Query query)
        {
            return await Mediator.Send(query);
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<PaginatedProductsDto>> GetProductsList([FromQuery] GetProductsList.Query query)
        {
            return await Mediator.Send(query);
        }

        [HttpPost("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductDetailDto>> CreateProduct([FromBody] CreateProduct.Command command)
        {
            return await Mediator.Send(command);
        }

        [HttpPut("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductDetailDto>> UpdateProduct([FromBody] UpdateProduct.Command command)
        {
            return await Mediator.Send(command);
        }

        [HttpDelete("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductDetailDto>> DeleteProduct([FromBody] DeleteProduct.Command command)
        {
            return await Mediator.Send(command);
        }
    }
}
