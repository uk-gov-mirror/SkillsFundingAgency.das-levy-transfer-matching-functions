using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.LevyTransferMatching.Functions.Api;
using SFA.DAS.LevyTransferMatching.Functions.Events;
using SFA.DAS.LevyTransferMatching.Functions.Messages.Commands;

namespace SFA.DAS.LevyTransferMatching.Functions.UnitTests.EventHandlers;

[TestFixture]
public class ApprenticeshipEmployerTypeChangeEventHandlerTests
{
    private readonly Fixture _fixture = new();

    [Test]
    public async Task Handle_For_NonLevy_Fans_Out_Commands_For_All_Active_Pledges()
    {
        // Arrange
        var accountId = _fixture.Create<long>();
        var @event = new ApprenticeshipEmployerTypeChangeEvent
        {
            AccountId = accountId,
            ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
            Created = DateTime.UtcNow
        };

        var api = new Mock<ILevyTransferMatchingApi>();
        api.Setup(x => x.GetActivePledgeIdsForAccount(accountId, 1, 100))
            .ReturnsAsync(new GetActivePledgeIdsForAccountResponse
            {
                PledgeIds = [11, 12],
                Page = 1,
                TotalPages = 2
            });
        api.Setup(x => x.GetActivePledgeIdsForAccount(accountId, 2, 100))
            .ReturnsAsync(new GetActivePledgeIdsForAccountResponse
            {
                PledgeIds = [13],
                Page = 2,
                TotalPages = 2
            });

        var handler = new ApprenticeshipEmployerTypeChangeEventHandler(api.Object, Mock.Of<ILogger<ApprenticeshipEmployerTypeChangeEventHandler>>());
        var context = new TestableMessageHandlerContext();

        // Act
        await handler.Handle(@event, context);

        // Assert
        context.SentMessages.Should().HaveCount(3);
        context.SentMessages.Select(m => ((ProcessPledgeNonLevyCleanupCommand)m.Message).PledgeId).Should().BeEquivalentTo(new[] { 11, 12, 13 });
    }

    [Test]
    public async Task Handle_For_Levy_Does_Not_Process()
    {
        // Arrange
        var @event = new ApprenticeshipEmployerTypeChangeEvent
        {
            AccountId = _fixture.Create<long>(),
            ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy,
            Created = DateTime.UtcNow
        };

        var api = new Mock<ILevyTransferMatchingApi>();

        var handler = new ApprenticeshipEmployerTypeChangeEventHandler(api.Object, Mock.Of<ILogger<ApprenticeshipEmployerTypeChangeEventHandler>>());
        var context = new TestableMessageHandlerContext();

        // Act
        await handler.Handle(@event, context);

        // Assert
        api.Verify(x => x.GetActivePledgeIdsForAccount(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        context.SentMessages.Should().BeEmpty();
    }
}
