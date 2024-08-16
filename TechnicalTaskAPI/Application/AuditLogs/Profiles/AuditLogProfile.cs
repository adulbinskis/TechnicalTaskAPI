using AutoMapper;
using TechnicalTaskAPI.Application.AuditLogs.Models;
using TechnicalTaskAPI.ORM.Entities;

namespace TechnicalTaskAPI.Application.AuditLogs.Profiles
{
    public class AuditLogProfile : Profile
    {
        public AuditLogProfile() 
        {
            CreateMap<AuditLog, AuditLogDto>();
        }
    }
}
