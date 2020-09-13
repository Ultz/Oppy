using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ultz.Oppy.Content
{
    public interface IContent
    {
        Task HandleAsync(HttpContext ctx, Func<Task> next);
    }
}