namespace One.Server.Hubs;

using Microsoft.AspNetCore.SignalR;

using One.Server.Services;

using System;

public class UpgradeHub : Hub
{
    private const string DASHBOARD_GROUP = "Dashboard";
    private const string DEVICE_GROUP = "Devices";

    private readonly ClientStateManager _deviceService;

    public UpgradeHub(ClientStateManager deviceService)
    {
        _deviceService = deviceService;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext()!;
        var role = httpContext.Request.Query["role"].ToString();

        var connectionId = Context.ConnectionId;

        await Clients.Caller.SendAsync("ReceiveMessage", "Hub is here!");

        if (role == "dashboard")
        {
            await Groups.AddToGroupAsync(connectionId, DASHBOARD_GROUP);
        }
        else
        {
            await Groups.AddToGroupAsync(connectionId, DEVICE_GROUP);
            MatchClientStateManager(httpContext, connectionId);

            await Clients.Caller.SendAsync("Online", $"Hub => <{connectionId}> is now online.");
        }

        // 🔔 只广播给 Dashboard
        await Clients.Group(DASHBOARD_GROUP)
            .SendAsync("ClientOnline", new
            {
                ConnectionId = Context.ConnectionId
            });

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _deviceService.RemoveByConnectionId(Context.ConnectionId);

        // 🔔 只广播给 Dashboard
        await Clients.Group(DASHBOARD_GROUP)
            .SendAsync("ClientOffline", new
            {
                ConnectionId = Context.ConnectionId
            });

        await base.OnDisconnectedAsync(exception);
    }

    private void MatchClientStateManager(HttpContext httpContext, string connectionId)
    {
        string? token = null;

        var authHeader = httpContext?.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(authHeader) &&
            authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = authHeader.Substring("Bearer ".Length).Trim();
        }

        if (!string.IsNullOrEmpty(token))
        {
            Context.Items["token"] = token;
            _deviceService.BindConnectionByToken(token, connectionId);
        }
    }
}