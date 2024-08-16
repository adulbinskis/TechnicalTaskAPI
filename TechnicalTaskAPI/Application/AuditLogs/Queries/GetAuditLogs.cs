using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnicalTaskAPI.Application.AuditLogs.Models;
using TechnicalTaskAPI.ORM.Entities;
using TechnicalTaskAPI.ORM.Services;

namespace TechnicalTaskAPI.Application.AuditLogs.Queries
{
    public class GetAuditLogs : IRequestHandler<GetAuditLogs.Query, List<AuditLogDto>>
    {
        public class Query : IRequest<List<AuditLogDto>>
        {
            public int? Count { get; set; }
            public DateTimeOffset DateFrom { get; set; }
            public DateTimeOffset DateTo { get; set; }
            public class Validator : AbstractValidator<Query>
            {
                public Validator()
                {
                    RuleFor(x => x.DateFrom).NotEmpty();
                    RuleFor(x => x.DateTo).NotEmpty();
                }
            }
        }

        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetAuditLogs(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<AuditLogDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var auditCount = request.Count ?? 10;

            IQueryable<AuditLog> query = _dbContext.AuditLogs
                .Where(x => x.CreatedAt > request.DateFrom)
                .Where(x => x.CreatedAt < request.DateTo)
                .OrderByDescending(x => x.CreatedAt)
                .Take(auditCount);

            var auditLogs = await query.ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<AuditLogDto>>(auditLogs);

            return dtos;
        }
    }
}
