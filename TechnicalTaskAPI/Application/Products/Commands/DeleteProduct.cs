using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechnicalTaskAPI.Application.Products.Models;
using TechnicalTaskAPI.ORM.Services;

namespace TechnicalTaskAPI.Application.Products.Commands
{
    public class DeleteProduct : IRequestHandler<DeleteProduct.Command, ProductDetailDto>
    {
        public class Command : IRequest<ProductDetailDto>
        {
            public Guid ProductId { get; set; }

            public class Validator : AbstractValidator<Command>
            {
                public Validator()
                {
                    RuleFor(x => x.ProductId).NotEmpty();
                }
            }
        }

        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DeleteProduct(ApplicationDbContext dbContext, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ProductDetailDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId);

            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("User ID not found in token.");
            }

            product.Deleted = true;

            _dbContext.Update(product);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var productDto = _mapper.Map<ProductDetailDto>(product);

            return productDto;
        }
    }
}
