namespace GeneralUpdate.Server.Controllers;

using GeneralUpdate.Server.DeviceManager;
using GeneralUpdate.Server.DTOs;
using GeneralUpdate.Server.Hubs;
using GeneralUpdate.Server.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

[ApiController]
[Route("api/report")]
public class ReportController : ControllerBase
{
    private readonly DeviceSessionService _deviceService;
    private readonly IHubContext<UpgradeHub> _hub;

    public ReportController(DeviceSessionService deviceService, IHubContext<UpgradeHub> hub)
    {
        _deviceService = deviceService;
        _hub = hub;
    }

    [HttpPost("online")]
    public async Task<IActionResult> Online([FromBody] DeviceOnlineDto dto)
    {
        var session = new DeviceSession
        {
            DeviceId = dto.DeviceId,
            HostName = dto.HostName,
            AppName = dto.AppName,
            Version = dto.Version,
            Ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        };

        _deviceService.AddOrUpdate(session);

        //await _hub.Clients.All.SendAsync(
        //    "Online",
        //    $"{dto.DeviceId} is online");

        return Ok();
    }

    [HttpGet("list")]
    public IActionResult List()
        => Ok(_deviceService.GetAll());
}