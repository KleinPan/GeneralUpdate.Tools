namespace One.Server.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using One.Server.DeviceManager;
using One.Server.DTOs;
using One.Server.Hubs;

[ApiController]
[Route("api/client")]
public class ReportController : ControllerBase
{
    private readonly ClientStateManager _deviceService;
    private readonly IHubContext<UpgradeHub> _hub;

    public ReportController(ClientStateManager deviceService, IHubContext<UpgradeHub> hub)
    {
        _deviceService = deviceService;
        _hub = hub;
    }

    [HttpPost("heartbeat")]
    public async Task<IActionResult> status([FromBody] ClientHeartbeatDTO dto)
    {
        var session = new DeviceSession
        {
            ClientID = dto.ClientID,
            Token = dto.Token,
            HostName = dto.HostName,
            AppName = dto.AppName,
            Version = dto.Version,
            OnlineTime = dto.OnlineTime,
            Ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        };

        _deviceService.AddOrUpdate(session);

        return Ok();
    }

    [HttpGet("list")]
    public IActionResult List()
        => Ok(_deviceService.GetAll());
}