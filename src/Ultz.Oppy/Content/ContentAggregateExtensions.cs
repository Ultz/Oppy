using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ultz.Oppy.Content
{
    public static class ContentAggregateExtensions
    {
        public static Func<HttpContext, Task>? Aggregate(this IReadOnlyList<IContent> handlers)
        {
            return handlers.Aggregate(0);
        }

        private static Func<HttpContext, Task>? Aggregate(this IReadOnlyList<IContent> handlers, int index)
        {
            if (index == handlers.Count) return null;

            var currentHandler = handlers[index];
            var nextHandler = handlers.Aggregate(index + 1);

            return context => currentHandler.HandleAsync(context, () => nextHandler?.Invoke(context) ?? Task.CompletedTask);
        }
    }
}