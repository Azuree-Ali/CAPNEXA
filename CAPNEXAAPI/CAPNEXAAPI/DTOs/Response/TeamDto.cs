namespace CAPNEXAAPI.DTOs.Response;

public sealed class TeamDto
{
    public int TeamId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string TeamCode { get; init; } = string.Empty;
    public int MaxMembers { get; init; }
    public int MemberCount { get; init; }
    public IReadOnlyList<TeamMemberDto> Members { get; init; } = [];
}

public sealed class TeamMemberDto
{
    public string UserId { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string StudentNumber { get; init; } = string.Empty;
    public bool IsLeader { get; init; }
    public DateTime JoinedAt { get; init; }
}
