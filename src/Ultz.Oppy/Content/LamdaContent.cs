using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ultz.Oppy.Content
{
    public class LamdaContent : IContent
    {
        private readonly Func<HttpContext, Func<Task>, Task> _handler;

        public LamdaContent(Func<HttpContext, Func<Task>, Task> handler)
        {
            _handler = handler;
        }

        public Task HandleAsync(HttpContext ctx, Func<Task> next) => _handler(ctx, next);
    }
}