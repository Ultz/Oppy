// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ultz.Oppy.Core
{
    /// <inheritdoc />
    public class Listener : IListener
    {
        /// <summary>
        /// Creates a listener from the given <see cref="ILoggerFactory" />.
        /// </summary>
        /// <remarks>
        /// Should only be used by Microsoft Dependency Injection.
        /// </remarks>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory" /> to use.</param>
        public Listener(ILoggerFactory? loggerFactory)
        {
            LoggerFactory = loggerFactory;
        }

        /// <inheritdoc />
        public List<Host> Hosts { get; } = new List<Host>();

        /// <inheritdoc />
        public ILoggerFactory? LoggerFactory { get; }

        /// <inheritdoc />
        public Task HandleAsync(HttpContext context)
        {
            foreach (var host in Hosts)
            {
                if ((host.Names is null || host.Names.Contains(context.Request.Host.Host)) &&
                    host.Listeners.ContainsKey((ushort) context.Connection.LocalPort) ||
                    (host.Names?.Any(x => x.Trim() == "*") ?? true) || host.Names?.Length == 0)
                {
                    return host.Content.HandleAsync(context);
                }
            }

            return Task.CompletedTask;
        }
    }
}