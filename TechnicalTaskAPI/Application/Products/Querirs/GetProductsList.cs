using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnicalTaskAPI.Application.Products.Models;
using TechnicalTaskAPI.ORM.Entities;
using TechnicalTaskAPI.ORM.Services;

namespace TechnicalTaskAPI.Application.Products.Querirs
{
    public class GetProductsList : IRequestHandler<GetProductsList.Query, PaginatedProductsDto>
    {
        public class Query : IRequest<PaginatedProductsDto>
        {
            public string SearchCriteria { get; set; }
            public int Page { get; set; }
        }

        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetProductsList(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<PaginatedProductsDto> Handle(Query request, CancellationToken cancellationToken)
        {
            IQueryable<Product> query = _dbContext.Products
                .Where(x => !x.Deleted)
                .OrderByDescending(x => x.CreatedAt);

            if (!string.IsNullOrWhiteSpace(request.SearchCriteria))
            {
                string searchLower = request.SearchCriteria.ToLower();
                query = query.Where(q =>
                    q.Name.ToLower().Contains(searchLower));
            }

            int pageSize = 8;
            int offset = (request.Page - 1) * pageSize;

            var products = await query
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<ProductDetailDto>>(products);

            int totalProductsCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalProductsCount / pageSize);

            return new PaginatedProductsDto
            {
                Products = dtos,
                TotalPages = totalPages
            };
        }
    }
}
