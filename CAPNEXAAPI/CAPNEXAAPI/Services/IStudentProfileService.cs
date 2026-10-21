using CAPNEXAAPI.DTOs.Request;
using CAPNEXAAPI.DTOs.Response;

namespace CAPNEXAAPI.Services;

public enum ProfileOperationError
{
    None,
    NotFound,
    Forbidden,
    Conflict,
    InvalidRequest,
    ServiceUnavailable
}

public sealed record ProfileOperationResult<T>(
    T? Value,
    ProfileOperationError Error = ProfileOperationError.None,
    string? ErrorCode = null)
{
    public bool Succeeded => Error == ProfileOperationError.None;
}

public interface IStudentProfileService
{
    Task<ProfileOperationResult<StudentProfileDto>> GetOwnProfileAsync(string userId);
    Task<ProfileOperationResult<StudentProfileDto>> UpdateOwnProfileAsync(string userId, UpdateStudentProfileDto request);
    Task<ProfileOperationResult<bool>> ChangeOwnPasswordAsync(string userId, ChangeStudentPasswordDto request);
    Task<ProfileOperationResult<StudentProfileDto>> CompleteOwnProfileAsync(string userId, CompleteStudentProfileDto request);
    Task<ProfileOperationResult<bool>> RequestEmailChangeAsync(string userId, string newEmail);
    Task<ProfileOperationResult<bool>> ConfirmEmailChangeAsync(string userId, string newEmail, string token);
    Task<ProfileOperationResult<StudentProfileDto>> GetStudentProfileForAdminAsync(string userId);
}
