// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ultz.Oppy.Content
{
    /// <summary>
    /// Represents a HTTP content handler.
    /// </summary>
    public interface IHandler
    {
        /// <summary>
        /// Performs ahead-of-time steps for the given HTTP path and disk path.
        /// </summary>
        /// <param name="oppyPath">The desired path to this file on the server.</param>
        /// <param name="diskPath">The path to this file on disk.</param>
        /// <param name="next">The next <see cref="LoadFileAsync" /> method in the chain of responsibility.</param>
        /// <returns>An asynchronous task.</returns>
        /// <remarks>
        /// <para>
        /// Implementations should <c>await next();</c> unless they have an explicit reason for not doing so. This is
        /// just in case the <see cref="HandleAsync" /> can't handle a given context. Do not <c>await next();</c> if
        /// allowing another handler access to the file (and potentially letting it leak out to the public) poses a
        /// security risk.
        /// </para>
        /// </remarks>
        Task LoadFileAsync(string oppyPath, string diskPath, Func<Task> next);

        /// <summary>
        /// Handles the given <see cref="HttpContext" />.
        /// </summary>
        /// <param name="ctx">The context to handle.</param>
        /// <param name="next">The next <see cref="HandleAsync" /> method in the chain of responsibility.</param>
        /// <returns>An asynchronous task.</returns>
        /// <remarks>
        /// <para>
        /// Implementations should <c>await next();</c> if they are unable to handle the given context, so that the next
        /// handler can try to handle it.
        /// </para>
        /// </remarks>
        Task HandleAsync(HttpContext ctx, Func<Task> next);
    }
}