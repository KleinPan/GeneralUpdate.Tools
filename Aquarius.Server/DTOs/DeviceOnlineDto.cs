namespace GeneralUpdate.Server.DTOs;

public class DeviceOnlineDto
{
    public string DeviceId { get; set; } = default!;
    public string HostName { get; set; } = default!;

    public string AppName { get; set; } = default!;
    public string Version { get; set; } = default!;
}