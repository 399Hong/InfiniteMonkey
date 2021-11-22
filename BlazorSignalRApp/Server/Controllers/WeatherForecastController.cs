using BlazorSignalRApp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BlazorSignalRApp.Server.Hubs;

using Microsoft.AspNetCore.SignalR;

namespace BlazorSignalRApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {   
        private readonly IHubContext<ChatHub> _Hub;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,IHubContext<ChatHub> Hub)
        {
            this.logger = logger;
             _Hub=Hub;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost("/top")]
        public async Task Post( TopRequest tr)
        {       
            Console.WriteLine(tr.genome);
            await _Hub.Clients.All.SendAsync("ReceiveMessage",tr);
            return;
        }


    }
}
