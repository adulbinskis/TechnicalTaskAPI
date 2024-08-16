using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TechnicalTaskAPI.Application.AuditLogs.Models;
using TechnicalTaskAPI.Application.AuditLogs.Queries;

namespace TechnicalTaskAPI.Application._Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogController : MediatRController
    {
        [HttpGet("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<AuditLogDto>>> AuditLogs([FromQuery] GetAuditLogs.Query query)
        {
            return await Mediator.Send(query);
        }
    }
}
