namespace One.Server.DTOs;

public class ClientHeartbeatDTO
{
    public string ClientID { get; set; } = default!;
    public string Token { get; set; } = default!;
    public string HostName { get; set; } = default!;

    public string AppName { get; set; } = default!;
    public string Version { get; set; } = default!;

    public string Message { get; set; } = default!;
}