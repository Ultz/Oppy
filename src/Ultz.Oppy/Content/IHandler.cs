using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Ultz.Oppy.Core;

namespace Ultz.Oppy.Content
{
    public interface IHandler
    {
        Task LoadFileAsync(string oppyPath, string diskPath, Func<Task> next);
        Task HandleAsync(HttpContext ctx, Func<Task> next);
    }
}