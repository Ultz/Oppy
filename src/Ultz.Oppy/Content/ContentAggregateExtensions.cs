// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ultz.Oppy.Content
{
    /// <summary>
    /// Contains extensions for aggregating a collection of handlers into one set of recursive delegates, to achieve the
    /// chain of responsibility flow that lies at the heart of Oppy and its handlers.
    /// </summary>
    public static class ContentAggregateExtensions
    {
        /// <summary>
        /// Aggregates the given collection of handlers into one set of recursive delegates.
        /// </summary>
        /// <param name="handlers">The handlers to aggregate.</param>
        /// <param name="handleAsync">The aggregated <see cref="IHandler.HandleAsync" /> method.</param>
        /// <param name="loadFileAsync">The aggregated <see cref="IHandler.LoadFileAsync" /> method.</param>
        /// <param name="last">
        /// The delegate to be called if no handler in the aggregated <see cref="IHandler.HandleAsync" /> method
        /// completes. May be null, in which case the last handler will just return <see cref="Task.CompletedTask" />.
        /// </param>
        public static void Aggregate(this IReadOnlyList<IHandler> handlers, out Func<HttpContext, Task>? handleAsync,
            out Func<string, string, Task>? loadFileAsync, Func<HttpContext, Task>? last)
        {
            handlers.Aggregate(0, out handleAsync, out loadFileAsync, last);
        }

        private static void Aggregate(this IReadOnlyList<IHandler> handlers, int index,
            out Func<HttpContext, Task>? handleAsync, out Func<string, string, Task>? loadFileAsync,
            Func<HttpContext, Task>? last)
        {
            handleAsync = null;
            loadFileAsync = null;
            if (index == handlers.Count)
            {
                return;
            }

            var currentHandler = handlers[index];
            handlers.Aggregate(index + 1, out var nextHandler, out var nextLoadFile, last);

            handleAsync = context =>
                currentHandler.HandleAsync(context,
                    () => nextHandler?.Invoke(context) ?? last?.Invoke(context) ?? Task.CompletedTask);
            loadFileAsync = (oppyPath, diskPath) =>
                currentHandler.LoadFileAsync(oppyPath, diskPath,
                    () => nextLoadFile?.Invoke(oppyPath, diskPath) ?? Task.CompletedTask);
        }
    }
}