namespace CSN.Infrastructure.Models.EmployeeDto;

public class EmployeeRegisterRequest
{
    public string Login { get; set; } = null!;
    public string Invite { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string? Image { get; set; } = null!;
}
