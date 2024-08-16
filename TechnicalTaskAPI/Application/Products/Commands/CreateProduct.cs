using AutoMapper;
using FluentValidation;
using MediatR;
using System.Security.Claims;
using TechnicalTaskAPI.Application.Products.Models;
using TechnicalTaskAPI.ORM.Entities;
using TechnicalTaskAPI.ORM.Services;

namespace TechnicalTaskAPI.Application.Products.Commands
{
    public class CreateProduct : IRequestHandler<CreateProduct.Command, ProductDetailDto>
    {
        public class Command : IRequest<ProductDetailDto>
        {
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

        public CreateProduct(ApplicationDbContext dbContext, IMapper mapper, IBaseEntityService baseEntityService, IHttpContextAccessor httpContextAccessor)
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

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Quantity = request.Quantity,
                PricePerUnit = request.PricePerUnit,
            };

            await _baseEntityService.SetCreatedPropertiesAsync(product);

            _dbContext.Products.Add(product);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            var productDto = _mapper.Map<ProductDetailDto>(product);

            return productDto;
        }
    }
}
