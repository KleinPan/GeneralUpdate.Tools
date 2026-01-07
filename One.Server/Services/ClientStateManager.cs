namespace One.Server.Services;

using One.Server.DeviceManager;

using System.Collections.Concurrent;
using System.Text.Json;

public class ClientStateManager
{
    private readonly ConcurrentDictionary<string, DeviceSession> _devices = new();

    public bool AddOrUpdate(DeviceSession session)
    {
        return _devices.AddOrUpdate(
                      session.ClientID,
                      session,
                      (_, _) => session) is not null;
    }

    public void BindConnection(string clientId, string connectionId)
    {
        if (_devices.TryGetValue(clientId, out var state))
        {
            state.ConnectionId = connectionId;
        }
    }

    public void BindConnectionByToken(string token, string connectionId)
    {
        var client = _devices.Values.FirstOrDefault(c => c.Token == token);

        client.ConnectionId = connectionId;
    }

    public bool RemoveByDeviceId(string clientID)
        => _devices.TryRemove(clientID, out _);

    public bool RemoveByConnectionId(string connectionId)
    {
        var item = _devices.FirstOrDefault(x => x.Value.ConnectionId == connectionId);
        return !item.Equals(default(KeyValuePair<string, DeviceSession>))
            && _devices.TryRemove(item.Key, out _);
    }

    public DeviceSession? Get(string clientID)
        => _devices.TryGetValue(clientID, out var d) ? d : null;

    public string GetAll()
    {
        var all = _devices.Values.ToList();
        return JsonSerializer.Serialize(all);
    }
}