namespace One.Server.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using One.Server.DeviceManager;
using One.Server.DTOs;
using One.Server.Extensions;

[ApiController]
[Route("api/client")]
public class ReportController : ControllerBase
{
    private readonly ClientStateManager _deviceService;
    private readonly ILogger<ReportController> _logger;

    public ReportController(
        ClientStateManager deviceService,
        ILogger<ReportController> logger)
    {
        _deviceService = deviceService;
        _logger = logger;
    }

    /// <summary>心跳接口：只更新 Status 和 StartTime，轻量级心跳</summary>
    [HttpPost("heartbeat")]
    public IActionResult Heartbeat([FromBody] ClientHeartbeatDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.AppID))
        {
            _logger.LogWarningWithTime("Heartbeat received with empty AppID");
            return BadRequest("AppID is required");
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        _deviceService.Heartbeat(dto.AppID, dto.Status, dto.CurrentTime, ip);
        _logger.LogDebugWithTime($"Heartbeat from {dto.AppID}: Status={dto.Status}");

        return Ok();
    }

    /// <summary>报告接口：上报完整设备信息（MachineInfo、AppInfo、InstanceInfo）</summary>
    [HttpPost("report")]
    public IActionResult Report([FromBody] ClientReportDTO dto)
    {
        var appId = dto.AppInfo?.AppID;

        if (string.IsNullOrWhiteSpace(appId))
        {
            _logger.LogWarningWithTime("Report received with empty AppID");
            return BadRequest("AppID is required");
        }

        _deviceService.UpdateReportInfo(
            appId,
            dto.MachineInfo,
            dto.AppInfo,
            dto.InstanceInfo);

        _logger.LogDebugWithTime("Report from {AppId}", appId);

        return Ok();
    }

    [HttpGet("list")]
    public IActionResult List()
        => Ok(_deviceService.GetAll());
}