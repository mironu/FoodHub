using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;




namespace FoodHub.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AndroidHub : Hub
    {
        public async Task SendMessage(object[] args)
        {
            await Clients.All.SendAsync("ReceiveMessage", "a", "b");

        }

    }
}

