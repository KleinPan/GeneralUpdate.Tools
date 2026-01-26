namespace One.Server.DTOs;

public class ClientHeartbeatDTO
{
    public string MachineID { get; set; } = string.Empty;
    public string AppID { get; set; } = string.Empty;

    /// <summary>0-abnormal,1-Normal</summary>
    public int Status { get; set; } // Online / Busy / Error

    public DateTime CurrentTime { get; set; }
}

public class ClientReportDTO
{
    public MachineInfoM MachineInfo { get; set; } = new();
    public AppInfoM AppInfo { get; set; } = new();

    public InstanceInfoM InstanceInfo { get; set; } = new();
}

public class MachineInfoM
{
    public string MachineID { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public string OS { get; set; } = string.Empty;
    public string MAC { get; set; } = string.Empty;
    public string LocalIPv4 { get; set; } = string.Empty;
    public string LocalIPv6 { get; set; } = string.Empty;
    public string IP { get; set; } = string.Empty;

    /// <summary>动态扩展字段</summary>
    public Dictionary<string, object> Extra { get; set; } = new Dictionary<string, object>();
}

public class AppInfoM
{
    public string AppID { get; set; } = string.Empty;

    public string AppName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;

    /// <summary>动态扩展字段</summary>
    public Dictionary<string, object> Extra { get; set; } = new Dictionary<string, object>();
}

public class InstanceInfoM
{
    public DateTime StartTime { get; set; } = DateTime.Now;
    public string ProcessId { get; set; } = string.Empty;

    /// <summary>动态扩展字段</summary>
    public Dictionary<string, object> Extra { get; set; } = new Dictionary<string, object>();
}

public class RuntimeInfo
{
    public string Status { get; set; } = string.Empty;
    public string CpuUsage { get; set; } = string.Empty;
    public string Memory { get; set; } = string.Empty;
}