using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechnicalTaskAPI.Application.Products.Models;
using TechnicalTaskAPI.ORM.Services;

namespace TechnicalTaskAPI.Application.Products.Commands
{
    public class UpdateProduct : IRequestHandler<UpdateProduct.Command, ProductDetailDto>
    {
        public class Command : IRequest<ProductDetailDto>
        {
            public Guid ProductId { get; set; }
            public string Name { get; set; }
            public int Quantity { get; set; }
            public decimal PricePerUnit { get; set; }

            public class Validator : AbstractValidator<Command>
            {
                public Validator()
                {
                    RuleFor(command => command.Name)
                        .NotEmpty().WithMessage("Name is required.")
                        .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

                    RuleFor(command => command.Quantity)
                        .GreaterThanOrEqualTo(0).WithMessage("Quantity must be greater than or equal to 0.");

                    RuleFor(command => command.PricePerUnit)
                        .GreaterThan(0).WithMessage("Price per unit must be greater than 0.");
                }
            }
        }

        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IBaseEntityService _baseEntityService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpdateProduct(ApplicationDbContext dbContext, IMapper mapper, IBaseEntityService baseEntityService, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _baseEntityService = baseEntityService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ProductDetailDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("User ID not found in token.");
            }

            var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId);

            if (product == null)
            {
                throw new Exception($"Product with ID {request.ProductId} not found.");
            }

            product.Name = request.Name;
            product.Quantity = request.Quantity;
            product.PricePerUnit = request.PricePerUnit;

            await _baseEntityService.SetUpdatedPropertiesAsync(product);

            _dbContext.Products.Update(product);

            await _dbContext.SaveChangesAsync(cancellationToken);

            var productDto = _mapper.Map<ProductDetailDto>(product);

            return productDto;
        }
    }
}
