using GeneralUpdate.Common.Models;
using One.Server.DTOs;
using One.Server.Hubs;
using One.Server.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using System.Globalization;
using System.Net.Sockets;
using System.Text.Json;

namespace One.Server.Controllers;

[ApiController]
[Route("api/upgrade")]
public class UpgradeControler : ControllerBase
{
    private readonly ClientStateManager _deviceService;
    private readonly IHubContext<UpgradeHub> _hub;
    private readonly IWebHostEnvironment _env;

    public UpgradeControler(IWebHostEnvironment env, ClientStateManager deviceService, IHubContext<UpgradeHub> hub)
    {
        _env = env;
        _deviceService = deviceService;
        _hub = hub;
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
            Console.WriteLine(DateTime.Now);
            Console.WriteLine(JsonSerializer.Serialize(dto));

            var req = HttpContext.Request;
            var fileDir = Path.Combine(_env.WebRootPath, "packages");

            var packList = Directory.GetFiles(fileDir).Where(x => x.EndsWith(".json"));

            if (dto.AppType == 1)//ClientApp
            {
                List<VerificationResultDTO> verificationResultDTOs = new();
                foreach (var item in packList)
                {
                    var content = System.IO.File.ReadAllText(item);
                    var currentVersion = JsonSerializer.Deserialize<VersionInfoM>(content)!;
                    //var packet = new FileInfo(item);

                    verificationResultDTOs.Add(new VerificationResultDTO()
                    {
                        RecordId = 1,
                        Name = currentVersion.PacketName,
                        Hash = currentVersion.Hash,
                        ReleaseDate = DateTime.ParseExact(currentVersion.BuildTime, "yyyyMMddHH_mmssfff", CultureInfo.InvariantCulture),
                        Url = $"{req.Scheme}://{req.Host}/packages/{currentVersion.PacketName}{currentVersion.Format}",
                        Version = currentVersion.Version,
                        AppType = 1,
                        Platform = 1,
                        ProductId = "",
                        IsForcibly = false,
                        Format = currentVersion.Format,
                        //Size = packet.Length,
                        IsFreeze = false
                    });
                }

                return HttpResponseDTO<IEnumerable<VerificationResultDTO>>.Success(verificationResultDTOs, $"{DateTime.Now} has update.");
            }
            else if (dto.AppType == 2)//2:UpdateApp
            {
                return HttpResponseDTO<IEnumerable<VerificationResultDTO>>.Success(null, $"{DateTime.Now} has update.");
            }
            else
            {
                return HttpResponseDTO<IEnumerable<VerificationResultDTO>>.Failure("AppType error!");
            }
        }
        catch (Exception ex)
        {
            return HttpResponseDTO<IEnumerable<VerificationResultDTO>>.Failure(ex.ToString());
        }
    }
}