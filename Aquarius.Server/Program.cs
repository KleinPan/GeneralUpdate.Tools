using GeneralUpdate.Server.Hubs;
using GeneralUpdate.Server.Services;

namespace GeneralUpdate.Server;

public class Program
{
    public static void Main(string[] args)
    {
        //https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/minimal-apis/webapplication?view=aspnetcore-10.0
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddSingleton<DeviceSessionService>();
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        // Ìí¼Ó SignalR ·þÎñ
        builder.Services.AddSignalR();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseStaticFiles();
        app.MapControllers();

        app.MapHub<UpgradeHub>("/UpgradeHub");

        app.Run();
    }
}