namespace SFA.DAS.LevyTransferMatching.Functions.Api;

public class GetActivePledgeIdsForAccountResponse
{
    public IEnumerable<int> PledgeIds { get; set; } = [];
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalPledges { get; set; }
}
