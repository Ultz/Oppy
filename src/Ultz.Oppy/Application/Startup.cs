// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.

using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ultz.Extensions.Logging;
using Ultz.Oppy.Core;

namespace Ultz.Oppy.Application
{
    /// <summary>
    /// Default startup class for Oppy servers.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// This method gets called by the runtime, and is used to add services to the container.
        /// For more information on how this works, visit https://go.microsoft.com/fwlink/?LinkID=398940
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
        }

        /// <summary>
        /// This method gets called by the runtime, and is used to configure the HTTP request pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app)
        {
            app.RunOppy();
        }

        /// <summary>
        /// The main entry-point for the application.
        /// </summary>
        /// <param name="args">The command line arguments the application was executed with.</param>
        public static void Main(string[] args)
        {
            var webHostBuilder = new WebHostBuilder();
            webHostBuilder
                .UseOppy(listener =>
                    listener.WithHosts(OppyInstallation.Hosts.Select(x => new Host(in x, listener)).ToArray()))
                .ConfigureLogging(logging => logging.ClearProviders().AddUltzLogger())
                .UseStartup<Startup>()
                .Build()
                .Run();
        }
    }
}