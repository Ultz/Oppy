using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ultz.Oppy.Content
{
    public static class ContentAggregateExtensions
    {
        public static void Aggregate(this IReadOnlyList<IHandler> handlers, out Func<HttpContext, Task>? handleAsync,
            out Func<string, string, Task>? loadFileAsync) => handlers.Aggregate(0, out handleAsync, out loadFileAsync);

        private static void Aggregate(this IReadOnlyList<IHandler> handlers, int index,
            out Func<HttpContext, Task>? handleAsync, out Func<string, string, Task>? loadFileAsync)
        {
            handleAsync = null;
            loadFileAsync = null;
            if (index == handlers.Count) return;

            var currentHandler = handlers[index];
            handlers.Aggregate(index + 1, out var nextHandler, out var nextLoadFile);

            handleAsync = context =>
                currentHandler.HandleAsync(context, () => nextHandler?.Invoke(context) ?? Task.CompletedTask);
            loadFileAsync = (oppyPath, diskPath) =>
                currentHandler.LoadFileAsync(oppyPath, diskPath,
                    () => nextLoadFile?.Invoke(oppyPath, diskPath) ?? Task.CompletedTask);
        }
    }
}