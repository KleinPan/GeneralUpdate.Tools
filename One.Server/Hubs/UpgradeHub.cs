using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using One.Server.DeviceManager;
using One.Server.DTOs;
using One.Server.Extensions;
using System;
using System.Collections.Generic;

namespace One.Server.Hubs;

public class UpgradeHub : Hub
{
    private const string DashboardGroup = "Dashboard";
    private const string DeviceGroup = "Devices";

    private readonly ClientStateManager _deviceService;
    private readonly ILogger<UpgradeHub> _logger;

    // 记录每个 appId 请求对应的 Dashboard ConnectionId
    private static readonly Dictionary<string, List<string>> _requestMap = new();

    private static readonly object _lock = new();

    public UpgradeHub(ClientStateManager deviceService, ILogger<UpgradeHub> logger)
    {
        _deviceService = deviceService;
        _logger = logger;
    }

    #region Connection Management

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext() ?? throw new InvalidOperationException("HttpContext is null");
        var role = httpContext.Request.Query["role"].ToString();
        var connectionId = Context.ConnectionId;

        _logger.LogInformationWithTime("Client connected: {ConnectionId}, role: {Role}", connectionId, role);

        await Clients.Caller.SendAsync("ReceiveMessage", "Hub is here!");

        if (role.Equals("dashboard", StringComparison.OrdinalIgnoreCase))
        {
            await Groups.AddToGroupAsync(connectionId, DashboardGroup);
            _logger.LogDebugWithTime("Client {ConnectionId} added to Dashboard group", connectionId);
        }
        else
        {
            await Groups.AddToGroupAsync(connectionId, DeviceGroup);

            var appKey = GetAppKey(httpContext);
            var session = _deviceService.BindConnectionByID(appKey, connectionId);

            if (session != null)
            {
                await Clients.Caller.SendAsync("Online", $"Hub => <{appKey}> is now online.");

                // 广播给 Dashboard
                await Clients.Group(DashboardGroup)
                    .SendAsync("ClientOnline", session);

                _logger.LogInformationWithTime("Device {AppKey} is now online", appKey);
            }
            else
            {
                _logger.LogWarningWithTime("Device {AppKey} not found when binding connection", appKey);
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var session = _deviceService.RemoveByConnectionId(Context.ConnectionId, out var removedSession);

        if (exception != null)
            _logger.LogErrorWithTime(exception, "Client {ConnectionId} disconnected with error", Context.ConnectionId);
        else
            _logger.LogInformationWithTime("Client {ConnectionId} disconnected", Context.ConnectionId);

        if (removedSession != null)
        {
            await Clients.Group(DashboardGroup)
                .SendAsync("ClientOffline", removedSession);

            _logger.LogDebugWithTime("Device {AppId} is now offline", removedSession.AppInfo.AppID);
        }

        await base.OnDisconnectedAsync(exception);
    }

    #endregion

    #region Dashboard Methods

    /// <summary>获取客户端应用信息（仪表盘调用）- 根据 AppID</summary>
    public async Task RequestClientAppInfo(string appId)
    {
        try
        {
            var session = _deviceService.Get(appId);

            if (session == null || session.ConnectionId == null)
            {
                _logger.LogWarningWithTime("Device {AppId} not found when requesting app info", appId);
                await Clients.Caller.SendAsync("ClientAppInfoResult", new Dictionary<string, object>
                {
                    { "runtime", "设备离线" },
                    { "memory", "N/A" },
                    { "cpu", "N/A" }
                });
                return;
            }

            // 记录 Dashboard ConnectionId
            lock (_lock)
            {
                if (!_requestMap.ContainsKey(appId))
                    _requestMap[appId] = new List<string>();

                if (!_requestMap[appId].Contains(Context.ConnectionId))
                    _requestMap[appId].Add(Context.ConnectionId);
            }


            // 向目标客户端发送采集请求
            await Clients.Client(session.ConnectionId).SendAsync("GetAppInfo", appId);
            _logger.LogInformationWithTime("Requested app info for device {AppId}", appId);
        }
        catch (Exception ex)
        {
            _logger.LogErrorWithTime(ex, "Failed to request app info for device {AppId}", appId);
            await Clients.Caller.SendAsync("ClientAppInfoResult", new Dictionary<string, object>
            {
                { "runtime", "获取失败" },
                { "memory", "N/A" },
                { "cpu", "N/A" }
            });
        }
    }

    /// <summary>
    /// 客户端回传应用信息
    /// </summary>
    public async Task SendAppInfoResult(string appId, Dictionary<string, object> appInfo)
    {
        List<string>? dashboardIds = null;

        lock (_lock)
        {
            if (_requestMap.TryGetValue(appId, out var list))
            {
                dashboardIds = new List<string>(list);
                _requestMap.Remove(appId);
            }
        }

        if (dashboardIds != null)
        {
            foreach (var dashId in dashboardIds)
            {
                await Clients.Client(dashId).SendAsync("ClientAppInfoResult", appInfo);
            }
            _logger.LogInformationWithTime("App info forwarded for device {AppId} to {Count} dashboards", appId, dashboardIds.Count);
        }
        else
        {
            _logger.LogWarningWithTime("No Dashboard mapping found for appId {AppId}", appId);
        }

    }

    #endregion

    #region Helper Methods

    private string GetToken(HttpContext httpContext)
    {
        var authHeader = httpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authHeader))
            throw new ArgumentException("Authorization header is missing");

        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Invalid Authorization header format");

        return authHeader.Substring("Bearer ".Length).Trim();
    }

    private string GetAppKey(HttpContext httpContext)
    {
        var key = httpContext.Request.Headers["appkey"].ToString();

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("AppKey header is missing");

        return key.Trim();
    }

    #endregion
}
