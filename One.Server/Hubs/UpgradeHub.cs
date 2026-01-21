namespace One.Server.Hubs;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

using One.Server.DeviceManager;

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

        //Console.WriteLine($"OnConnectedAsync ConnectionId : {connectionId}");
        await Clients.Caller.SendAsync("ReceiveMessage", "Hub is here!");

        if (role == "dashboard")
        {
            await Groups.AddToGroupAsync(connectionId, DASHBOARD_GROUP);
        }
        else
        {
            await Groups.AddToGroupAsync(connectionId, DEVICE_GROUP);

            var token = GetToken(httpContext);//每次打开生成
            var appID = GetAPPKey(httpContext);//APPID
            //Context.Items["token"] = token;
            var session = _deviceService.BindConnectionByToken(token, connectionId);

 

            await Clients.Caller.SendAsync("Online", $"Hub => <{appID}> is now online.");

            // 🔔 只广播给 Dashboard
            await Clients.Group(DASHBOARD_GROUP)
                .SendAsync("ClientOnline", session);
                
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _deviceService.RemoveByConnectionId(Context.ConnectionId, out DeviceSession session);


        //Console.WriteLine($"OnDisconnectedAsync ConnectionId : {Context.ConnectionId}");

        //var session = SetOffineLine(  connectionId);
        //// 🔔 只广播给 Dashboard

        if (session == null)
        {
            return;
        }
        //await Clients.Group(DASHBOARD_GROUP)
        //    .SendAsync("ClientOffline", new
        //    {
        //        HostName = session.MachineInfo.HostName,
        //        ClientID = session.AppInfo.AppID,
        //        IP = session.Ip,
        //        LastSeen = DateTime.UtcNow
        //    });
        await Clients.Group(DASHBOARD_GROUP)
           .SendAsync("ClientOffline", session);

        await base.OnDisconnectedAsync(exception);
    }

    #region Helper

    private string GetToken(HttpContext httpContext)
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
            return token;
        }
        else
        {
            throw new Exception("token is null!");
        }
    }
    private string GetAPPKey(HttpContext httpContext)
    {
        string? token = null;

        var key = httpContext?.Request.Headers["appkey"].ToString();
        if (!string.IsNullOrEmpty(key))
        {

            return key.Trim();
        }


        else
        {
            throw new Exception("key is null!");
        }
    }



    #endregion Helper
}