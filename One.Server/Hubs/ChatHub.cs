using Microsoft.AspNetCore.SignalR;

namespace One.Server.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(string message)
    {
        //await Clients.All.SendAsync("ReceiveMessage", user, message);

        //await Clients.Caller.SendAsync("SendMessage", "Hub Send=> Connected");
        await Clients.Caller.SendAsync("ReceiveMessage", message);
    }

    public async Task ReceiveMessage(string message)
    {

        Console.WriteLine(message);
        //await Clients.Caller.SendAsync("ReceiveMessage", message);
    }
}