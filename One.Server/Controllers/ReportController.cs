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
    public async Task<IActionResult> Heartbeat([FromBody] ClientHeartbeatDTO dto)
    {

        var session = new DeviceSession
        {
            MachineInfo = dto.MachineInfo,
            AppInfo = dto.AppInfo,
            InstanceInfo = dto.InstanceInfo,
            

            Ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        };

        _deviceService.AddOrUpdate(session);

        return Ok();
    }

    [HttpGet("list")]
    public IActionResult List()
        => Ok(_deviceService.GetAll());
}