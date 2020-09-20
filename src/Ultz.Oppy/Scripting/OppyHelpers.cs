using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Ultz.Oppy.Content;

namespace Ultz.Oppy.Scripting
{
    public static class OppyHelpers
    {
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Example CSX file using this:
        /// <code>
        /// OppyHelpers.Handler
        /// (
        ///     async (context, nextHandler) =>
        ///     {
        ///         Console.WriteLine("Hello!");
        ///         await nextHandler();
        ///     }
        /// );
        /// </code>
        /// </remarks>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static HandlerScript Handler(Func<HttpContext, Func<Task>, Task> handler,
            PathMatchingMode matchingMode = PathMatchingMode.Default)
            => new HandlerScript(handler, matchingMode);
    }
}