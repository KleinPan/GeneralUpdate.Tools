namespace One.Server.Hubs;

using One.Server.Services;

using Microsoft.AspNetCore.SignalR;

using System;

public class UpgradeHub : Hub
{
    private readonly ClientStateManager _deviceService;

    public UpgradeHub(ClientStateManager deviceService)
    {
        _deviceService = deviceService;
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("ReceiveMessage", "Hub => Connected");

        var connectionId = Context.ConnectionId;
        await Clients.Caller.SendAsync("Online", $"Hub => <{connectionId}> is now online.");

        var httpContext = Context.GetHttpContext()!;

        MatchClientStateManager(httpContext, connectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _deviceService.RemoveByConnectionId(Context.ConnectionId);
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