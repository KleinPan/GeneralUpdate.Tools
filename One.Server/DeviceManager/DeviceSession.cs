using One.Server.DTOs;

namespace One.Server.DeviceManager;

public class DeviceSession
{
    public MachineInfo MachineInfo { get; set; } = new MachineInfo();
    public AppInfo AppInfo { get; set; } = new AppInfo();
    public InstanceInfo InstanceInfo { get; set; } = new InstanceInfo();


    public string Ip { get; set; } = default!;




    /// <summary>当前 SignalR 连接（可为空）</summary>
    public string? ConnectionId { get; set; }

    public string? Message { get; set; }

}