using TechnicalTaskAPI.Application.Services.Interfaces;

namespace TechnicalTaskAPI.Application.Services
{
    public class DateTimeService : IDateTimeService
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
