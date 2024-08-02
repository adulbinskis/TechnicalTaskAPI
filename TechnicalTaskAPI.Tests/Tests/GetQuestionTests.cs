using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TechnicalTaskAPI.Application.Identity.Commands;
using TechnicalTaskAPI.Tests.Fixtures;

namespace TechnicalTaskAPI.Tests.Tests
{
    public class GetQuestionTests : IClassFixture<DatabaseFixture>
    {
        private readonly IMediator _mediator;

        public GetQuestionTests(DatabaseFixture fixture)
        {
            _mediator = fixture.ServiceProvider.GetRequiredService<IMediator>();
        }

        [Fact]
        public async Task TestGetQuestion_Success()
        {
            // Arrange 
            var command = new Authenticate.Command { Email = "armandsdulbinskis@gmail.com", Password = "P@ssw0rd" };

            // Act
            var result = await _mediator.Send(command);

            // Assert
            Assert.NotNull(result);
        }
    }
}
