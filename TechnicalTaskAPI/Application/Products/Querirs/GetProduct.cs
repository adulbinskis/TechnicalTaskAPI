using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnicalTaskAPI.Application.Products.Models;
using TechnicalTaskAPI.ORM.Services;

namespace TechnicalTaskAPI.Application.Products.Querirs
{
    public class GetProduct : IRequestHandler<GetProduct.Query, ProductDetailDto>
    {
        public class Query : IRequest<ProductDetailDto>
        {
            public Guid ProductId { get; set; }

            public class Validator : AbstractValidator<Query>
            {
                public Validator()
                {
                    RuleFor(x => x.ProductId).NotEmpty();
                }
            }
        }

        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetProduct(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<ProductDetailDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var product = await _dbContext.Products
                .Where(x => !x.Deleted)
                .Where(q => q.Id == request.ProductId)
                .FirstOrDefaultAsync(cancellationToken);

            if (product == null)
            {
                throw new Exception("Product not found");
            }

            var productDetailDto = _mapper.Map<ProductDetailDto>(product);
            return productDetailDto;
        }
    }
}
