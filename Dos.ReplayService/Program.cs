using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Dos.ReplayService
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                 {
                     webBuilder
                        .UseStartup<Startup>()
                        .UseWebRoot("front")
                        .UseUrls("http://*:" + GetPort());
                 });

        private static string GetPort()
        {
            var port = Environment.GetEnvironmentVariable("PORT");

            return string.IsNullOrEmpty(port) ? "5423" : port;
        }
    }
}
