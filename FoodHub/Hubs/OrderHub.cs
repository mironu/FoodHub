using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace FoodHub.Hubs
{
    public class OrderHub : Hub
    {
        public async Task SendMessage(object[] args)
        {
            await Clients.All.SendAsync("ReceiveMessage", "a", "b");
          
        }       

    }
}