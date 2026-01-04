using GeneralUpdate.Server.DTOs;
using GeneralUpdate.Server.Hubs;
using GeneralUpdate.Server.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using System.Text.Json;

namespace GeneralUpdate.Server.Controllers;

[ApiController]
[Route("api/upgrade")]
public class UpgradeControler : ControllerBase
{
    private readonly DeviceSessionService _deviceService;
    private readonly IHubContext<UpgradeHub> _hub;

    public UpgradeControler(DeviceSessionService deviceService, IHubContext<UpgradeHub> hub)
    {
        _deviceService = deviceService;
        _hub = hub;
    }

    [HttpPost("report")]
    public async Task<HttpResponseDTO> report([FromBody] ReportDTO dto)
    {
        return HttpResponseDTO<bool>.Success(true, "has update.");
    }

    [HttpPost("verification")]
    public async Task<HttpResponseDTO> verification([FromBody] VerifyDTO dto)
    {
        try
        {
            Console.WriteLine(DateTime.Now);
            Console.WriteLine(JsonSerializer.Serialize(dto));
            var fileDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "packages");

            var packList = Directory.GetFiles(fileDir);

            var packet = new FileInfo("1");

            if (dto.AppType == 1)//ClientApp
            {
                var result = new List<VerificationResultDTO>
            {
                new VerificationResultDTO
                {
                    RecordId = 1,
                    Name = packet.Name,
                    Hash = "ad1a85a9169ca0083ab54ba390e085c56b9059efc3ca8aa1ec9ed857683cc4b1",
                    ReleaseDate = DateTime.Now,
                    Url = $"http://localhost:5000/packages/{packet.Name}.zip",
                    Version = "1.3.1.1",
                    AppType = 1,
                    Platform = 1,
                    ProductId = "2d974e2a-31e6-4887-9bb1-b4689e98c77a",
                    IsForcibly = false,
                    Format = ".zip",
                    Size = packet.Length,
                    IsFreeze = false
                }
            };
                return HttpResponseDTO<IEnumerable<VerificationResultDTO>>.Success(result, $"{DateTime.Now} has update.");
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