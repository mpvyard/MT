﻿using Microsoft.AspNetCore.Hosting;

namespace MarginTrading.RiskManagerBroker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://*:5007")
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}