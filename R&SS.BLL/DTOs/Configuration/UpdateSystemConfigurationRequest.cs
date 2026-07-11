namespace R_SS.BLL.DTOs.Configuration;

public class UpdateSystemConfigurationRequest
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public string? Description { get; set; }
    public int ActorUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
}
