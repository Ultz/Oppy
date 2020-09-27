// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.
// 

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Ultz.Oppy.Content;

namespace Ultz.Oppy.Scripting
{
    /// <summary>
    /// Contains helpers intended to be used by scripts to make interoperation with the Oppy handler flow easy.
    /// </summary>
    public static class OppyHelpers
    {
        /// <summary>
        /// Creates a handler from the given delegate and, optionally, path matching mode.
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
        /// <param name="handler">The handler delegate to use.</param>
        /// <param name="matchingMode">The default path matching mode to use.</param>
        /// <returns>A scripted handler object.</returns>
        public static HandlerScript Handler(Func<HttpContext, Func<Task>, Task> handler,
            PathMatchingMode matchingMode = PathMatchingMode.Default)
        {
            return new HandlerScript(handler, matchingMode);
        }

        /// <summary>
        /// Marks this handler as immune to <see cref="ScriptHandler" /> mixins and protections, and instead registers
        /// the handler as a standalone handler.
        /// </summary>
        /// <param name="handler">The handler to mark as a standalone handler.</param>
        /// <returns>An encapsulation, marking the handler as a standalone handler.</returns>
        public static NoMixinEncapsulation NoMixin(IHandler handler)
        {
            return new NoMixinEncapsulation(handler);
        }
    }
}