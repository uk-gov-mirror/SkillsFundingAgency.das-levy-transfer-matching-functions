using System.Linq;
using NServiceBus;
using RestEase;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.LevyTransferMatching.Functions.Api;
using SFA.DAS.LevyTransferMatching.Functions.Messages.Commands;

namespace SFA.DAS.LevyTransferMatching.Functions.Events;

public class ApprenticeshipEmployerTypeChangeEventHandler(ILevyTransferMatchingApi api, ILogger<ApprenticeshipEmployerTypeChangeEventHandler> log)
    : IHandleMessages<ApprenticeshipEmployerTypeChangeEvent>
{
    public async Task Handle(ApprenticeshipEmployerTypeChangeEvent @event, IMessageHandlerContext context)
    {
        if (@event.ApprenticeshipEmployerType != ApprenticeshipEmployerType.NonLevy)
        {
            log.LogInformation(
                "Ignoring ApprenticeshipEmployerTypeChangeEvent for account {AccountId} because employer type is {EmployerType}",
                @event.AccountId,
                @event.ApprenticeshipEmployerType);
            return;
        }

        log.LogInformation("Handling ApprenticeshipEmployerTypeChangeEvent for account {AccountId}", @event.AccountId);

        var page = 1;
        var totalPages = 1;
        var commandsSent = 0;

        try
        {
            while (page <= totalPages)
            {
                var response = await api.GetActivePledgeIdsForAccount(@event.AccountId, page);
                totalPages = response?.TotalPages > 0 ? response.TotalPages : 0;

                foreach (var pledgeId in response?.PledgeIds?.Distinct() ?? [])
                {
                    await context.SendLocal(new ProcessPledgeNonLevyCleanupCommand
                    {
                        AccountId = @event.AccountId,
                        PledgeId = pledgeId
                    });

                    commandsSent++;
                }

                page++;
            }

            log.LogInformation(
                "Queued {CommandsSent} non-levy pledge cleanup commands for account {AccountId}",
                commandsSent,
                @event.AccountId);
        }
        catch (ApiException ex)
        {
            log.LogError(ex, "Error handling ApprenticeshipEmployerTypeChangeEvent for account {AccountId}", @event.AccountId);
            throw;
        }
    }
}
