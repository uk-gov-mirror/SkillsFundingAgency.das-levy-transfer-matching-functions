using NServiceBus;
using RestEase;
using SFA.DAS.LevyTransferMatching.Functions.Api;
using SFA.DAS.LevyTransferMatching.Functions.Messages.Commands;

namespace SFA.DAS.LevyTransferMatching.Functions.Commands;

public class ProcessPledgeNonLevyCleanupCommandHandler(ILevyTransferMatchingApi api, ILogger<ProcessPledgeNonLevyCleanupCommandHandler> log)
    : IHandleMessages<ProcessPledgeNonLevyCleanupCommand>
{
    public async Task Handle(ProcessPledgeNonLevyCleanupCommand command, IMessageHandlerContext context)
    {
        log.LogInformation(
            "Running non-levy cleanup for account {AccountId}, pledge {PledgeId}",
            command.AccountId,
            command.PledgeId);

        var request = new CleanupPledgeForNonLevyRequest
        {
            AccountId = command.AccountId,
            PledgeId = command.PledgeId
        };

        try
        {
            await api.CleanupPledgeForNonLevy(request);
        }
        catch (ApiException ex)
        {
            log.LogError(
                ex,
                "Failed non-levy cleanup for account {AccountId}, pledge {PledgeId}",
                command.AccountId,
                command.PledgeId);
            throw;
        }
    }
}
