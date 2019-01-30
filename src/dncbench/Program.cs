using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

//using Microsoft.AspNetCore.Server.HttpSys;

namespace dncbench
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        // http.sys
        /*
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseHttpSys(options =>
                {
                    // The following options are set to default values.
                    options.Authentication.Schemes = AuthenticationSchemes.None;
                    options.Authentication.AllowAnonymous = true;
                    options.MaxConnections = null;
                    options.MaxRequestBodySize = 30000000;
                });
        */


        // Unix socket for Nginx+Kestrel 
        /*
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseLibuv()
                .UseStartup<Startup>()
                .ConfigureKestrel((context, options) =>
                {
                    options.ListenUnixSocket("/tmp/api.sock");
                });
        */
    }
}
