namespace One.Server.DeviceManager;

using One.Server.DTOs;

using System.Collections.Concurrent;
using System.Text.Json;

public class ClientStateManager
{
    /// <summary>主表：AppID -&gt; DeviceSession</summary>
    private readonly ConcurrentDictionary<string, DeviceSession> _devices = new();

    /// <summary>反查表：ConnectionId -&gt; AppID，用于快速查找</summary>
    private readonly ConcurrentDictionary<string, string> _connectionMap = new();

    /// <summary>创建或更新心跳信息（只更新 Status 和 StartTime）</summary>
    /// <param name="appId">应用标识</param>
    /// <param name="status">状态</param>
    /// <param name="currentTime">启动时间</param>
    public void Heartbeat(string appId, int status, DateTime currentTime, string ip)
    {
        _devices.AddOrUpdate(
            appId,
            new DeviceSession
            {
                AppInfo = new AppInfoM { AppID = appId },
                Status = status,
                CurrentTime = currentTime,
            },
            (_, existing) =>
            {
                existing.Status = status;
                existing.CurrentTime = currentTime;
                existing.MachineInfo.IP = ip;

                return existing;
            });
    }

    /// <summary>创建设备会话（首次从 Report 创建）</summary>
    public void CreateFromReport(string appId, MachineInfoM machineInfo, AppInfoM appInfo, InstanceInfoM instanceInfo)
    {
        var session = new DeviceSession
        {
            MachineInfo = machineInfo,
            AppInfo = appInfo,
            InstanceInfo = instanceInfo,
        };

        if (string.IsNullOrEmpty(session.MachineInfo.IP))
        {
            session.MachineInfo.IP = "unknown";
        }

        _devices[appId] = session;
    }

    /// <summary>更新设备信息（从 Report 更新）</summary>
    public void UpdateReportInfo(string appId, MachineInfoM machineInfo, AppInfoM appInfo, InstanceInfoM instanceInfo)
    {
        if (_devices.TryGetValue(appId, out var existing))
        {
            existing.MachineInfo = machineInfo;
            existing.AppInfo = appInfo;
            existing.InstanceInfo = instanceInfo;
        }
        else
        {
            // 如果不存在，从 Report 创建
            CreateFromReport(appId, machineInfo, appInfo, instanceInfo);
        }
    }

    /// <summary>绑定连接ID（SignalR连接时调用）</summary>
    public DeviceSession? BindConnection(string appId, string connectionId)
    {
        if (_devices.TryGetValue(appId, out var session))
        {
            // 移除旧的连接映射
            if (session.ConnectionId != null)
            {
                _connectionMap.TryRemove(session.ConnectionId, out _);
            }

            session.ConnectionId = connectionId;
            _connectionMap[connectionId] = appId;
            return session;
        }
        return null;
    }

    /// <summary>根据连接ID绑定（由Hub调用）</summary>
    public DeviceSession? BindConnectionByID(string appId, string connectionId)
    {
        return BindConnection(appId, connectionId);
    }

    /// <summary>移除设备（按设备ID）</summary>
    public bool RemoveByDeviceId(string appId)
    {
        if (_devices.TryRemove(appId, out var session))
        {
            if (session.ConnectionId != null)
            {
                _connectionMap.TryRemove(session.ConnectionId, out _);
            }
            return true;
        }
        return false;
    }

    /// <summary>移除设备（按连接ID）- O(1) 复杂度</summary>
    public bool RemoveByConnectionId(string connectionId, out DeviceSession? session)
    {
        session = null;

        // 使用反查表快速找到 AppID
        if (!_connectionMap.TryRemove(connectionId, out var appId))
        {
            return false;
        }

        if (_devices.TryRemove(appId, out session))
        {
            return true;
        }

        return false;
    }

    /// <summary>获取设备会话（按设备ID）</summary>
    public DeviceSession? Get(string appId)
        => _devices.TryGetValue(appId, out var d) ? d : null;

    /// <summary>获取设备会话（按连接ID）- O(1) 复杂度</summary>
    public DeviceSession? GetByConnectionId(string connectionId)
    {
        if (_connectionMap.TryGetValue(connectionId, out var appId))
        {
            return _devices.TryGetValue(appId, out var session) ? session : null;
        }
        return null;
    }

    /// <summary>获取所有设备会话</summary>
    public string GetAll()
    {
        var all = _devices.Values.ToList();
        return JsonSerializer.Serialize(all);
    }
}