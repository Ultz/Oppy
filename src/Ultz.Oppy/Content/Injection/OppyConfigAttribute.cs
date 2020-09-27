// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.

using System;

namespace Ultz.Oppy.Content.Injection
{
    /// <summary>
    /// Instructs the needle to inject a JSON configuration value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class OppyConfigAttribute : Attribute
    {
        /// <summary>
        /// Constructs the attribute with the given JSON path to the configuration value.
        /// </summary>
        /// <param name="jPath">The JSON path to the configuration value.</param>
        public OppyConfigAttribute(string jPath)
        {
            JPath = jPath;
        }

        /// <summary>
        /// The JSON path to the desired configuration value.
        /// </summary>
        public string JPath { get; }
    }
}