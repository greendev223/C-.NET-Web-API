using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Trading.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //Use this section only for remote hosting.
                    //Also check the ip address you want to listen on. 
                    //The ip address must be "Preregister on the server
                    //See "Preregister URL prefixes on the server" on https://go.microsoft.com/fwlink/?linkid=2127065 for more information.
#pragma warning disable CA1416 // Validate platform compatibility
                    //webBuilder.UseHttpSys(options =>
                    //  {
                    //      options.UrlPrefixes.Add("https://*:443");
                    //      options.UrlPrefixes.Add("http://*:80");
                    //      options.Authentication.Schemes = Microsoft.AspNetCore.Server.HttpSys.AuthenticationSchemes.None;
                    //      options.Authentication.AllowAnonymous = true;
                    //      options.MaxConnections = null;
                    //      options.ClientCertificateMethod = Microsoft.AspNetCore.Server.HttpSys.ClientCertificateMethod.NoCertificate;
                    //  }).UseStartup<Startup>();
#pragma warning restore CA1416 // Validate platform compatibility

                    // Use this line only for local debugging.
                    webBuilder.UseStartup<Startup>();
                });
    }
}