using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using One.Server.DeviceManager;

using System;

namespace One.Server.Hubs;

public class UpgradeHub : Hub
{
    private const string DashboardGroup = "Dashboard";
    private const string DeviceGroup = "Devices";

    private readonly ClientStateManager _deviceService;
    private readonly ILogger<UpgradeHub> _logger;

    public UpgradeHub(ClientStateManager deviceService, ILogger<UpgradeHub> logger)
    {
        _deviceService = deviceService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext() ?? throw new InvalidOperationException("HttpContext is null");
        var role = httpContext.Request.Query["role"].ToString();
        var connectionId = Context.ConnectionId;

        _logger.LogInformation("Client connected: {ConnectionId}, role: {Role}", connectionId, role);

        await Clients.Caller.SendAsync("ReceiveMessage", "Hub is here!");

        if (role.Equals("dashboard", StringComparison.OrdinalIgnoreCase))
        {
            await Groups.AddToGroupAsync(connectionId, DashboardGroup);
            _logger.LogDebug("Client {ConnectionId} added to Dashboard group", connectionId);
        }
        else
        {
            await Groups.AddToGroupAsync(connectionId, DeviceGroup);

            var appKey = GetAppKey(httpContext);
            var session = _deviceService.BindConnectionByID(appKey, connectionId);

            if (session != null)
            {
                await Clients.Caller.SendAsync("Online", $"Hub => <{appKey}> is now online.");

                // 只广播给 Dashboard
                await Clients.Group(DashboardGroup)
                    .SendAsync("ClientOnline", session);

                _logger.LogInformation("Device {AppKey} is now online", appKey);
            }
            else
            {
                _logger.LogWarning("Device {AppKey} not found when binding connection", appKey);
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var session = _deviceService.RemoveByConnectionId(Context.ConnectionId, out var removedSession);

        if (exception != null)
        {
            _logger.LogWarning(exception, "Client {ConnectionId} disconnected with error", Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("Client {ConnectionId} disconnected", Context.ConnectionId);
        }

        if (removedSession != null)
        {
            // 只广播给 Dashboard
            await Clients.Group(DashboardGroup)
                .SendAsync("ClientOffline", removedSession);

            _logger.LogDebug("Device {AppId} is now offline", removedSession.AppInfo.AppID);
        }

        await base.OnDisconnectedAsync(exception);
    }

    #region Helper Methods

    /// <summary>从请求头获取 Bearer Token</summary>
    private string GetToken(HttpContext httpContext)
    {
        var authHeader = httpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authHeader))
        {
            throw new ArgumentException("Authorization header is missing");
        }

        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Invalid Authorization header format");
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentException("Token is empty");
        }

        return token;
    }

    /// <summary>从请求头获取 AppKey</summary>
    private string GetAppKey(HttpContext httpContext)
    {
        var key = httpContext.Request.Headers["appkey"].ToString();

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("AppKey header is missing");
        }

        return key.Trim();
    }

    #endregion
}