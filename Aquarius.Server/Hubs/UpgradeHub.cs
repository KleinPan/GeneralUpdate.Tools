namespace GeneralUpdate.Server.Hubs;

using GeneralUpdate.Server.Services;

using Microsoft.AspNetCore.SignalR;

using System;

public class UpgradeHub : Hub
{
    private readonly DeviceSessionService _deviceService;

    public UpgradeHub(DeviceSessionService deviceService)
    {
        _deviceService = deviceService;
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("ReceiveMessage", "Hub => Connected");

        var connectionId = Context.ConnectionId;
        await Clients.Caller.SendAsync("Online", $"Hub => <{connectionId}> is now online.");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _deviceService.RemoveByConnectionId(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}