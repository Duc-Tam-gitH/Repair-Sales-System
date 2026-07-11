namespace R_SS.BLL.DTOs.Configuration;

public class SystemConfigurationResponse
{
    public int SystemConfigurationId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public string? Description { get; set; }
    public string Message { get; set; } = string.Empty;
}
