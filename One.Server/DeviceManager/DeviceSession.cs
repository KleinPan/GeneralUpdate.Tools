namespace One.Server.DeviceManager;

public class DeviceSession
{
    public string ClientID { get; init; } = default!;
    public string HostName { get; init; } = default!;
    public string Ip { get; init; } = default!;
    public string Token { get; set; } = default!;

    public string AppName { get; init; }
    public string Version { get; init; } = default!;

    /// <summary>当前 SignalR 连接（可为空）</summary>
    public string? ConnectionId { get; set; }

    public string? Message { get; set; }
    public DateTime OnlineTime { get; init; }
}