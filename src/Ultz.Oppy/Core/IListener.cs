// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ultz.Oppy.Content;

namespace Ultz.Oppy.Core
{
    /// <summary>
    /// Represents an Oppy HTTP listener.
    /// </summary>
    public interface IListener
    {
        /// <summary>
        /// The logger factory for this listener. Used to construct <see cref="ILogger{TCategoryName}" />s for child objects, such
        /// as hosts, content registrars, and handlers.
        /// </summary>
        ILoggerFactory? LoggerFactory { get; }

        /// <summary>
        /// The hosts owned by this listener.
        /// </summary>
        List<Host> Hosts { get; }

        /// <summary>
        /// Routes this <see cref="HttpContext" /> to the correct <see cref="Host" /> and <see cref="ContentRegistrar" /> to be
        /// handled.
        /// </summary>
        /// <param name="context">The context to handle.</param>
        /// <returns>An asynchronous task.</returns>
        Task HandleAsync(HttpContext context);
    }
}