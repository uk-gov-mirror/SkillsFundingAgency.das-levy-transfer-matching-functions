using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.LevyTransferMatching.Functions.Api;
using SFA.DAS.LevyTransferMatching.Functions.Commands;
using SFA.DAS.LevyTransferMatching.Functions.Messages.Commands;

namespace SFA.DAS.LevyTransferMatching.Functions.UnitTests.Commands;

[TestFixture]
public class ProcessPledgeNonLevyCleanupCommandHandlerTests
{
    [Test]
    public async Task Handle_Invokes_CleanupPledgeForNonLevy_Api_Endpoint()
    {
        // Arrange
        var fixture = new Fixture();
        var command = fixture.Create<ProcessPledgeNonLevyCleanupCommand>();

        var api = new Mock<ILevyTransferMatchingApi>();

        var handler = new ProcessPledgeNonLevyCleanupCommandHandler(api.Object, Mock.Of<ILogger<ProcessPledgeNonLevyCleanupCommandHandler>>());

        // Act
        await handler.Handle(command, Mock.Of<IMessageHandlerContext>());

        // Assert
        api.Verify(x => x.CleanupPledgeForNonLevy(It.Is<CleanupPledgeForNonLevyRequest>(r =>
            r.AccountId == command.AccountId &&
            r.PledgeId == command.PledgeId)));
    }
}
