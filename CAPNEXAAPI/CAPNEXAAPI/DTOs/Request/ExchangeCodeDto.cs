using System.ComponentModel.DataAnnotations;

namespace CAPNEXAAPI.DTOs.Request;

public class ExchangeCodeDto
{
    [Required(ErrorMessage = "Code is required.")]
    public string Code { get; set; } = string.Empty;
}