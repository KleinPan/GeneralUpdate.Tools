namespace GeneralUpdate.Server.Services;

using GeneralUpdate.Server.DeviceManager;

using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

public class DeviceSessionService
{
    private readonly ConcurrentDictionary<string, DeviceSession> _devices = new();

    public bool AddOrUpdate(DeviceSession session)
    {
        return _devices.AddOrUpdate(
                      session.DeviceId,
                      session,
                      (_, _) => session) is not null;
    }

    public bool RemoveByDeviceId(string deviceId)
        => _devices.TryRemove(deviceId, out _);

    public bool RemoveByConnectionId(string connectionId)
    {
        var item = _devices.FirstOrDefault(x => x.Value.ConnectionId == connectionId);
        return !item.Equals(default(KeyValuePair<string, DeviceSession>))
            && _devices.TryRemove(item.Key, out _);
    }

    public DeviceSession? Get(string deviceId)
        => _devices.TryGetValue(deviceId, out var d) ? d : null;

    public string GetAll()
    {
        var all = _devices.Values.ToList();
        return JsonSerializer.Serialize(all);
    }
}