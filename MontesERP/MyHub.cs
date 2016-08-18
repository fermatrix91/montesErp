using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using MontesERP.Models.DAL;

namespace MontesERP
{
    public class MyHub : Hub
    {
        MontesERPEntities modeloErp = new MontesERPEntities();

        public void Hello()
        {
            Clients.All.hello("Este es un mensaje con SignalR");
        }

        public void TraerProducto()
        {
            Clients.All.traerProducto(modeloErp.Productoes.ToList());
        }

        public MyHub()
        {
            var taskTimer = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    string timeNow = DateTime.Now.ToString();
                    //Sending the server time to all the connected clients on the client method SendServerTime()
                    Clients.All.SendServerTime(timeNow);
                    //Delaying by 3 seconds.
                    await Task.Delay(3000);
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}