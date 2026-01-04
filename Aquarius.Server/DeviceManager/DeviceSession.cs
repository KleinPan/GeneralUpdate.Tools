namespace GeneralUpdate.Server.DeviceManager;

public class DeviceSession
{
    public string DeviceId { get; init; } = default!;
    public string HostName { get; init; } = default!;
    public string Ip { get; init; } = default!;

    public string AppName { get; init; }
    public string Version { get; init; } = default!;
    public string ConnectionId { get; set; } = default!;
    public DateTime OnlineTime { get; init; } = DateTime.Now;
}