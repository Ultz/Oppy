using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Ultz.Oppy.Configuration;

namespace Ultz.Oppy.Core
{
    public class Listener
    {
        public List<Host> Hosts { get; set; } = new List<Host>();

        public Listener AddHosts(params Host[] hosts)
        {
            Hosts.AddRange(hosts);
            return this;
        }

        internal Task HandleAsync(HttpContext context)
        {
            foreach (var host in Hosts)
            {
                if ((host.Names is null || host.Names.Contains(context.Request.Host.Host)) && host.Listeners.ContainsKey((ushort) context.Connection.LocalPort))
                {
                    return host.Content.HandleAsync(context);
                }
            }
        }
    }
}