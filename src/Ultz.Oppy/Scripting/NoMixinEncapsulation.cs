// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.
// 

using Ultz.Oppy.Content;

namespace Ultz.Oppy.Scripting
{
    /// <summary>
    /// An encapsulation which instructs the <see cref="ScriptHandler" /> not to mark this handler as a
    /// <see cref="HandlerScript" />, but instead as a standalone handler.
    /// </summary>
    public readonly struct NoMixinEncapsulation
    {
        /// <summary>
        /// The handler to be registered as a standalone handler.
        /// </summary>
        public IHandler Handler { get; }

        internal NoMixinEncapsulation(IHandler handler)
        {
            Handler = handler;
        }
    }
}