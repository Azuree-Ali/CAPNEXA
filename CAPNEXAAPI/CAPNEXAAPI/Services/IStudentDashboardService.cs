using CAPNEXAAPI.DTOs.Response;

namespace CAPNEXAAPI.Services;

public interface IStudentDashboardService
{
    Task<StudentDashboardResult<StudentDashboardDto>> GetDashboardAsync(string userId);
}

public enum StudentDashboardError
{
    None,
    ProfileRequired,
    AccountDisabled
}

public sealed record StudentDashboardResult<T>(T? Value, StudentDashboardError Error = StudentDashboardError.None)
{
    public bool Succeeded => Error == StudentDashboardError.None;
}