namespace One.Server.DTOs;

public class ClientHeartbeatDTO
{
    public MachineInfo MachineInfo { get; set; } = new MachineInfo();
    public AppInfo AppInfo { get; set; } = new AppInfo();
    public InstanceInfo InstanceInfo { get; set; } = new InstanceInfo();
    public RuntimeInfo RuntimeInfo { get; set; } = new RuntimeInfo();
}

public class MachineInfo
{
    public string MachineID { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public string OS { get; set; } = string.Empty;
}

public class AppInfo
{
    public string AppID { get; set; } = string.Empty;
    public string AppName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

public class InstanceInfo
{
    public string InstanceID { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public string ProcessId { get; set; } = string.Empty;
}

public class RuntimeInfo
{
    public string Status { get; set; } = string.Empty;
    public string CpuUsage { get; set; } = string.Empty;
    public string Memory { get; set; } = string.Empty;
}
