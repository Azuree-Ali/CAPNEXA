using System.ComponentModel.DataAnnotations;

namespace CAPNEXAAPI.DTOs.Request;

public class RegisterDto
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 100 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 100 characters.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "University email is required.")]
    [EmailAddress(ErrorMessage = "Invalid university email address.")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "University ID is required.")]
    [StringLength(50, ErrorMessage = "University ID must not exceed 50 characters.")]
    public string StudentNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department is required.")]
    [StringLength(100, ErrorMessage = "Department must not exceed 100 characters.")]
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "Academic year is required.")]
    [StringLength(20, ErrorMessage = "Academic year must not exceed 20 characters.")]
    public string AcademicYear { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password confirmation is required.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
