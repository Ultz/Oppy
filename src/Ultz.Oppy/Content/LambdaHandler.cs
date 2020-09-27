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
    /// A simple handler, backed by a generic delegate (lambda in most cases)
    /// </summary>
    public readonly struct LambdaHandler : IHandler
    {
        /// <summary>
        /// Creates a handler with the given delegated implementation of the <see cref="HandleAsync" /> method.
        /// </summary>
        /// <param name="handler"></param>
        public LambdaHandler(Func<HttpContext, Func<Task>, Task> handler)
        {
            Handler = handler;
        }

        /// <summary>
        /// The underlying handler delegate.
        /// </summary>
        public Func<HttpContext, Func<Task>, Task> Handler { get; }

        /// <inheritdoc />
        public async Task LoadFileAsync(string oppyPath, string diskPath, Func<Task> next)
        {
            await next();
        }

        /// <inheritdoc />
        public Task HandleAsync(HttpContext ctx, Func<Task> next)
        {
            return Handler(ctx, next);
        }
    }
}