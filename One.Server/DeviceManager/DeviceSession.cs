using One.Server.DTOs;

namespace One.Server.DeviceManager;

public class DeviceSession
{
    public int Status { get; set; } // Online / Busy / Error
    public DateTime CurrentTime { get; set; }

    public MachineInfoM MachineInfo { get; set; } = new();
    public AppInfoM AppInfo { get; set; } = new();
    public InstanceInfoM InstanceInfo { get; set; } = new();

    public RuntimeInfo RuntimeInfo { get; set; } = new();

    /// <summary>当前 SignalR 连接（可为空）</summary>
    public string? ConnectionId { get; set; }

    public string? Message { get; set; }
}