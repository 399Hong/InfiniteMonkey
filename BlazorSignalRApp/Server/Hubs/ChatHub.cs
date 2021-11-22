using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using BlazorSignalRApp.Shared;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

using System.Net.Http;

namespace BlazorSignalRApp.Server.Hubs
{
    public class ChatHub : Hub
    {   
    
        public async Task start(TargetRequest targetR,TryRequest tryR)
        {
             var client = new HttpClient ();
            
            client.BaseAddress = new Uri ("http://localhost:8091/");
            client.DefaultRequestHeaders.Accept.Clear ();
            client.DefaultRequestHeaders.Accept.Add (
                new MediaTypeWithQualityHeaderValue ("application/json"));
                
            Console.WriteLine ($"..... POST /target send {targetR.ToString()}");
            var hrm = await client.PostAsJsonAsync ("/target", targetR);
            hrm.EnsureSuccessStatusCode ();
            //await Clients.Caller.SendAsync("ReceiveMessage", user, message);
            //Console.WriteLine(test.genome);
            var client2 = new HttpClient ();
            
            client2.BaseAddress = new Uri ("http://localhost:8081/");
            client2.DefaultRequestHeaders.Accept.Clear ();
            client2.DefaultRequestHeaders.Accept.Add (
                new MediaTypeWithQualityHeaderValue ("application/json"));
                
            Console.WriteLine ($"..... POST /try send {tryR.ToString()}");
            var hrm2 = await client2.PostAsJsonAsync ("/try", tryR);
            hrm.EnsureSuccessStatusCode ();
        }
 
    }
}