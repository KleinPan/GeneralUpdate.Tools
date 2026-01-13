namespace One.Server.DeviceManager;

using System.Collections.Concurrent;
using System.Text.Json;

public class ClientStateManager
{
    /// <summary>主表：业务唯一</summary>
    private readonly ConcurrentDictionary<string, DeviceSession> _devices = new();

    // 反查表：连接维度
    private readonly ConcurrentDictionary<string, string> _connectionMap = new();

    /// <summary>
    /// 心跳调用
    /// </summary>
    /// <param name="session"></param>
    public void AddOrUpdate(DeviceSession session)
    {
        _devices.AddOrUpdate(
                    session.ClientID,
                    session,
                    (_, existing) =>
                    {
                        existing.Token= session.Token;
                        return existing;
                    });
    }

    public void BindConnection(string clientId, string connectionId)
    {
        if (_devices.TryGetValue(clientId, out var state))
        {
            state.ConnectionId = connectionId;
        }
    }
    /// <summary>
    /// hub连上后调用
    /// </summary>
    /// <param name="token"></param>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    public DeviceSession BindConnectionByToken(string token, string connectionId)
    {
        var client = _devices.Values.FirstOrDefault(c => c.Token == token);

        if (client==null)
        {
            return null;
        }
        client.ConnectionId = connectionId;

        return client;
    }

    public bool RemoveByDeviceId(string clientID)
        => _devices.TryRemove(clientID, out _);

    /// <summary>
    /// hub断开后调用
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    public bool RemoveByConnectionId(string connectionId,out DeviceSession deviceSession)
    {
        var item = _devices.FirstOrDefault(x => x.Value.ConnectionId == connectionId);
        deviceSession = item.Value;

        return !item.Equals(default(KeyValuePair<string, DeviceSession>))
            && _devices.TryRemove(item.Key, out _);
    }

    public DeviceSession? Get(string clientID)
        => _devices.TryGetValue(clientID, out var d) ? d : null;

    public DeviceSession? GetByConnectionID(string connectionId)
    {
        if (_connectionMap.TryGetValue(connectionId, out var clientId))
        {
            if (_devices.TryGetValue(clientId, out var session))
            {
                return session;
            }
        }

        return null;
    }

    public string GetAll()
    {
        var all = _devices.Values.ToList();
        return JsonSerializer.Serialize(all);
    }
}