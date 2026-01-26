using GeneralUpdate.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using One.Server.DeviceManager;
using One.Server.DTOs;
using One.Server.Hubs;

using System.Globalization;
using System.Text.Json;

namespace One.Server.Controllers;

/// <summary>
/// 应用类型常量
/// </summary>
public static class AppTypeConstants
{
    public const int ClientApp = 1;
    public const int UpdateApp = 2;
}

/// <summary>
/// 平台类型常量
/// </summary>
public static class PlatformConstants
{
    public const int Windows = 1;
}

[ApiController]
[Route("api/upgrade")]
public class UpgradeController : ControllerBase
{
    private readonly ClientStateManager _deviceService;
    private readonly IHubContext<UpgradeHub> _hub;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<UpgradeController> _logger;
    private readonly IMemoryCache _memoryCache;

    /// <summary>缓存键前缀</summary>
    private const string CACHE_KEY_PREFIX = "version_info_";

    public UpgradeController(
        IWebHostEnvironment env,
        ClientStateManager deviceService,
        IHubContext<UpgradeHub> hub,
        ILogger<UpgradeController> logger,
        IMemoryCache memoryCache)
    {
        _env = env;
        _deviceService = deviceService;
        _hub = hub;
        _logger = logger;
        _memoryCache = memoryCache;
    }

    [HttpPost("report")]
    public async Task<HttpResponseDTO> Report([FromBody] ReportDTO dto)
    {
        return HttpResponseDTO<bool>.Success(true, "has update.");
    }

    [HttpPost("verification")]
    public async Task<HttpResponseDTO> Verification([FromBody] VerifyDTO dto)
    {
        try
        {
            _logger.LogInformation("Verification request from {AppKey}, AppType: {AppType}",
                dto.AppKey, dto.AppType);

            var req = HttpContext.Request;
            var versionInfoList = GetVersionInfoList();

            if (dto.AppType == AppTypeConstants.ClientApp)
            {
                var results = versionInfoList.Select(currentVersion => new VerificationResultDTO
                {
                    RecordId = 1,
                    Name = currentVersion.PacketName,
                    Hash = currentVersion.Hash,
                    ReleaseDate = DateTime.ParseExact(
                        currentVersion.BuildTime,
                        "yyyyMMddHH_mmssfff",
                        CultureInfo.InvariantCulture),
                    Url = $"{req.Scheme}://{req.Host}/packages/{currentVersion.PacketName}{currentVersion.Format}",
                    Version = currentVersion.Version,
                    AppType = AppTypeConstants.ClientApp,
                    Platform = PlatformConstants.Windows,
                    ProductId = string.Empty,
                    IsForcibly = false,
                    Format = currentVersion.Format,
                    IsFreeze = false
                }).ToList();

                return HttpResponseDTO<IEnumerable<VerificationResultDTO>>.Success(
                    results,
                    "Verification completed.");
            }
            else if (dto.AppType == AppTypeConstants.UpdateApp)
            {
                return HttpResponseDTO<IEnumerable<VerificationResultDTO>>.Success(
                    Enumerable.Empty<VerificationResultDTO>(),
                    "No update required for UpdateApp.");
            }
            else
            {
                _logger.LogWarning("Invalid AppType: {AppType}", dto.AppType);
                return HttpResponseDTO<IEnumerable<VerificationResultDTO>>.Failure(
                    $"Invalid AppType: {dto.AppType}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Verification failed");
            return HttpResponseDTO<IEnumerable<VerificationResultDTO>>.Failure(
                $"Verification failed: {ex.Message}");
        }
    }

    #region Private Methods

    private List<VersionInfoM> GetVersionInfoList()
    {
        var fileDir = Path.Combine(_env.WebRootPath, "packages");
        var cacheKey = CACHE_KEY_PREFIX + fileDir;

        // 尝试从缓存获取
        var cachedData = _memoryCache.Get(cacheKey);
        if (cachedData is List<VersionInfoM> cachedVersionList)
        {
            _logger.LogDebug("Using cached version info ({Count} items)", cachedVersionList.Count);
            return cachedVersionList;
        }

        var packList = Directory.GetFiles(fileDir)
            .Where(x => x.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var resultList = new List<VersionInfoM>();

        foreach (var item in packList)
        {
            try
            {
                var content = System.IO.File.ReadAllText(item);
                var versionInfo = JsonSerializer.Deserialize<VersionInfoM>(content);
                if (versionInfo != null)
                {
                    resultList.Add(versionInfo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse version file: {File}", item);
            }
        }

        // 写入缓存，5分钟过期
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
            .SetSize(resultList.Count);

        _memoryCache.Set(cacheKey, resultList, cacheEntryOptions);

        _logger.LogInformation("Loaded {Count} version files from {Path}",
            resultList.Count, fileDir);

        return resultList;
    }

    #endregion
}